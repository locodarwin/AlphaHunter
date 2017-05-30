using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AW;

namespace AlphaHunter
{
    public partial class Form1 : Form
    {

        

        public Form1()
        {
            InitializeComponent();

            txtHost.Text = Globals.sUnivLogin;
            txtPort.Text = Convert.ToString(Globals.iPort);
            txtName.Text = Globals.sBotName;
            //txtDesc.Text = Globals.sBotDesc;
            txtCitNum.Text = Convert.ToString(Globals.iCitNum);
            txtPassword.Text = Globals.sPassword;
            txtWorld.Text = Globals.sWorld;
            txtCoords.Text = Globals.sCoords;
            //txtXPos.Text = Convert.ToString(Globals.iXPos);
            //txtYPos.Text = Convert.ToString(Globals.iYPos);
            //txtZPos.Text = Convert.ToString(Globals.iZPos);
            //txtYaw.Text = Convert.ToString(Globals.iYaw);
            txtAV.Text = Convert.ToString(Globals.iAV);

            aTimer = new Timer();
            aTimer.Tick += new EventHandler(aTimer_Tick);
            aTimer.Interval = 100;
            aTimer.Start();



            Status("Ready to log into universe & world.");
        }

        public IInstance m_bot;
        public Timer aTimer;

        public static class Globals
        {
            public static string sUnivLogin = "auth.activeworlds.com";
            public static int iPort = 6670;
            public static string sBotName = "AlphaHunter";
            public static string sBotDesc = "Bot Test";
            public static int iCitNum = 318855;
            public static string sPassword = "shamma11";
            public static string sWorld = "Simulator";
            public static string sCoords = "0n 0w 0a 0";
            public static int iXPos = 0;
            public static int iYPos = 0;
            public static int iZPos = 0;
            public static int iYaw = 0;
            public static int iAV = 1;

            public static bool iInUniv = false;
            public static bool iInWorld = false;
        }


        private void butLogin_Click(object sender, EventArgs e)
        {
            UnivLogin();
            WorldLogin();
        }






        // The click event for Login to Universe
        private void UnivLogin()
        {
            // Grab the contents of the controls and put them into the globals
            Globals.sUnivLogin = txtHost.Text;
            Globals.iPort = Convert.ToInt32(txtPort.Text);
            Globals.sBotName = txtName.Text;
            //Globals.sBotDesc = txtDesc.Text;
            Globals.iCitNum = Convert.ToInt32(txtCitNum.Text);
            Globals.sPassword = txtPassword.Text;


            // Check universe login state and abort if we're already logged in
            if (Globals.iInUniv == true)
            {
                Status("Already logged into universe!");
                return;
            }

            // Initalize the AW API?
            Status("Initializing the API instance.");
            m_bot = new Instance();

            // Install events & callbacks
            Status("Installing events and callbacks.");
            m_bot.EventAvatarAdd += OnEventAvatarAdd;
            m_bot.EventChat += OnEventChat;

            // Set universe login parameters
            m_bot.Attributes.LoginName = Globals.sBotName;
            m_bot.Attributes.LoginOwner = Globals.iCitNum;
            m_bot.Attributes.LoginPrivilegePassword = Globals.sPassword;
            m_bot.Attributes.LoginApplication = Globals.sBotDesc;

            // Log into universe
            Status("Entering universe.");
            var rc = m_bot.Login();
            if (rc != Result.Success)
            {
                Status("Failed to log in to universe (reason:" + rc + ").");
                return;
            }
            else
            {
                Status("Universe entry successful.");
                Globals.iInUniv = true;
            }
        }

        // Click event for logging into world
        private void WorldLogin()
        {

            // Check universe login state and abort if we're not already logged in
            if (Globals.iInUniv == false)
            {
                Status("Not logged into universe! Aborting.");
                return;
            }

            // Check world login state and abort if we're already logged in
            if (Globals.iInWorld == true)
            {
                Status("Already logged into world! Aborting.");
                return;
            }

            // Pull globals from controls for world entry and positioning
            Globals.sWorld = txtWorld.Text;
            //Globals.iXPos = Convert.ToInt32(txtXPos.Text);
            //Globals.iYPos = Convert.ToInt32(txtYPos.Text);
            //Globals.iZPos = Convert.ToInt32(txtZPos.Text);
            //Globals.iYaw = Convert.ToInt32(txtYaw.Text);

            // These are temporary - replace when done working on the coords converter
            Globals.iXPos = 0;
            Globals.iYPos = 0;
            Globals.iZPos = 0;
            Globals.iYaw = 0;

            Globals.iAV = Convert.ToInt32(txtAV.Text);

            // Enter world
            Status("Logging into world " + Globals.sWorld + ".");
            var rc = m_bot.Enter(Globals.sWorld);
            if (rc != Result.Success)
            {
                Status("Failed to log into world" + Globals.sWorld + " (reason:" + rc + ").");
                return;
            }
            else
            {
                Status("World entry successful.");
                Globals.iInWorld = true;
            }

            // Commit the positioning and become visible
            Status("Changing position in world.");
            m_bot.Attributes.MyX = Globals.iXPos;
            m_bot.Attributes.MyY = Globals.iYPos;
            m_bot.Attributes.MyZ = Globals.iZPos;
            m_bot.Attributes.MyYaw = Globals.iYaw;
            m_bot.Attributes.MyType = Globals.iAV;


            rc = m_bot.StateChange();
            if (rc == Result.Success)
            {
                Status("Movement successful.");
            }
            else
            {
                Status("Movement aborted (reason: " + rc + ").");
            }

        }




        private void OnEventAvatarAdd(IInstance sender)
        {
            sender.Whisper(sender.Attributes.AvatarSession, "Hello {0} and welcome to {1}!", sender.Attributes.AvatarName, sender.Attributes.WorldName);
        }


        private void OnEventChat(IInstance sender)
        {
            Status(sender.Attributes.AvatarName + ": " + sender.Attributes.ChatMessage);
            //
        }



        // Timer function for the AW Wait function
        private void aTimer_Tick(object source, EventArgs e)
        {
            Utility.Wait(0);
        }

        // The status update member
        private void Status(string sText)
        {
            lisStatus.Items.Add(sText);
            //lisStatus.SelectedItem = lisStatus.Items.Count - 1;
            lisStatus.TopIndex = lisStatus.Items.Count - 1;
            //listbox.TopIndex = listbox.Items.Count - 1;
        }



        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Globals.iInUniv == false)
            {
                return;
            }
            m_bot.Dispose();
            Status("Logged out.");
            Globals.iInUniv = false;
        }

        private void butLogout_Click_1(object sender, EventArgs e)
        {
            if (Globals.iInUniv == false)
            {
                Status("Not in universe. Aborted.");
                return;
            }
            m_bot.Dispose();
            Status("Logged out.");
            Globals.iInUniv = false;
            Globals.iInWorld = false;
        }
    }
}
