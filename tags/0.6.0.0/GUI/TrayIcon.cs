﻿ using System;
using System.Configuration;
using System.Windows.Forms;
using TaskLeader.BLL;
using TaskLeader.BO;
using System.Threading;

namespace TaskLeader.GUI
{
    public class TrayIcon: ApplicationContext
    {
        // Déclaration des composants IHM
        private static NotifyIcon trayIcon = new NotifyIcon();
        private ContextMenuStrip trayContext = new ContextMenuStrip();
        private ToolStripMenuItem newActionItem = new ToolStripMenuItem();
        private ToolStripMenuItem outlookItem = new ToolStripMenuItem();
        private ToolStripMenuItem closeItem = new ToolStripMenuItem();
        private ToolStripMenuItem maximItem = new ToolStripMenuItem();
        
        // Déclaration des composants métiers
        static Control invokeControl = new Control();

        // Déclaration de tous les composants
        private void loadComponents()
        {
            // trayIcon
            trayIcon.ContextMenuStrip = this.trayContext;
            trayIcon.Icon = Properties.Resources.task_coach;
            trayIcon.Text = "TaskLeader";
            trayIcon.Visible = true;
            trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);

            // Menu contextuel de la trayIcon
            this.trayContext.Items.AddRange(new ToolStripItem[] { this.newActionItem, this.maximItem, this.outlookItem, this.closeItem });
            this.trayContext.Name = "trayContext";
            this.trayContext.ShowImageMargin = false;
            this.trayContext.Size = new System.Drawing.Size(126, 70);
            this.trayContext.Opened += new EventHandler(trayContext_Opened);

            // Item "nouvelle action" du menu contextuel
            this.newActionItem.Name = "newActionItem";
            this.newActionItem.ShowShortcutKeys = false;
            this.newActionItem.Size = new System.Drawing.Size(125, 22);
            this.newActionItem.Text = "Nouvelle action";
            this.newActionItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.newActionItem.Click += new System.EventHandler(this.ajoutAction);

            // Item "afficher Toolbox" du menu contextuel
            this.maximItem.Name = "maximItem";
            this.maximItem.Size = new System.Drawing.Size(125, 22);
            this.maximItem.Text = "Afficher la liste";
            this.maximItem.Click += new System.EventHandler(this.maximItem_Click);

            // Item "nouvelle action" du menu contextuel
            this.outlookItem.Text = "Connecter à Outlook";
            this.outlookItem.Size = new System.Drawing.Size(125, 22);
            this.outlookItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.outlookItem.Click += new System.EventHandler(this.connectOutlook);

            // Item "fermer" du menu contextuel
            this.closeItem.Name = "closeItem";
            this.closeItem.Size = new System.Drawing.Size(125, 22);
            this.closeItem.Text = "Fermer";
            this.closeItem.Click += new System.EventHandler(this.closeItem_Click);
        }

        // Constructeur de la NotifyIcon
        public TrayIcon()
        {
            // On charge tous les composants
            this.loadComponents();

            // Vérification de démarrage
            if (Init.Instance.canLaunch())
            {
                this.displayToolbox(); // Affichage de la Toolbox
                invokeControl.CreateControl();
            }
            else
                this.closeApp(); // On ferme l'appli
        }

        private static Toolbox v_toolbox = null;
        
        // Méthode générique d'affichage de la Toolbox
        private void displayToolbox()
        {
            if (v_toolbox == null || v_toolbox.IsDisposed) // Si la fenêtre n'a jamais été ouverte ou fermée, on l'ouvre
            {
                v_toolbox = new Toolbox();
                v_toolbox.WindowState = FormWindowState.Maximized;
                v_toolbox.Show();
            }
            else
                v_toolbox.BringToFront(); // Sinon on l'affiche au premier plan     
        }
        
        // Update de la Toolbox si elle est affichée
        private static void updateToolbox(object sender, EventArgs e)
        {
            if(v_toolbox != null && !v_toolbox.IsDisposed)
                v_toolbox.miseAjour(sender,e);
        }
        
        // Double-click sur la trayIcon
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.displayToolbox();
        }

        // Méthode permettant d'afficher le formulaire nouvelle action vide
        private void ajoutAction(object sender, EventArgs e)
        {
            TLaction action = new TLaction();
            action.freezeInitState();

            ManipAction fenetre = new ManipAction(action);
            fenetre.Disposed += new EventHandler(updateToolbox); // Sur fermeture de ManipAction on update la Toolbox
            fenetre.Show();
        }

        //Délégué pour méthode newActionOutlook
        delegate void newActionOutlookCallback(TLaction action);
        // Méthode permettant d'afficher le formulaire nouvelle action avec les paramètres spécifiés
        public static void newActionOutlook(TLaction action)
        {
            if (invokeControl.InvokeRequired)
                invokeControl.Invoke(new newActionOutlookCallback(newActionOutlook), new object[] { action });
            else
            {
                ManipAction guiAction = new ManipAction(action);
                guiAction.TopMost = true;
                guiAction.Disposed += new EventHandler(updateToolbox);
                guiAction.Show();
            }           
        }

        // Activation si nécessaire de l'item outlook
        private void trayContext_Opened(object sender, EventArgs e)
        {
            this.outlookItem.Visible = OutlookIF.Instance.connectionNeeded;
        }

        // Tentative de connexion à Outlook
        private void connectOutlook(object sender, EventArgs e)
        {
            OutlookIF.Instance.tryHook(false);
        }

        // Demande d'affichage de la Toolbox via le ContextMenuStrip
        private void maximItem_Click(object sender, EventArgs e)
        {
            this.displayToolbox();
        }

        // Méthode générique de fermeture de l'appli
        private void closeApp()
        {
            trayIcon.Visible = false; 
            Application.Exit();
        }

        // Demande de fermeture via le ContextMenuStrip
        private void closeItem_Click(object sender, EventArgs e)
        {
            this.closeApp();
        }

        // Méthode pour affichage de message
        public static void afficheMessage(String titre, String info)
        {
            // Récupération de la durée d'affichage de la tooltip
            int duree = int.Parse(ConfigurationManager.AppSettings["dureeTooltip"]);

            // Affichage du bilan en tooltip de la tray icon
            trayIcon.ShowBalloonTip(duree, titre, info, ToolTipIcon.Info);
        }
    }
}
