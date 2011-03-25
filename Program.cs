﻿using System;
using System.Windows.Forms;
using TaskLeader.GUI;
using TaskLeader.BLL;

namespace TaskLeader
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);          

            //Hook d'outlook
            OutlookIF.Instance.hookOutlook();

            //Affichage de la TrayIcon
            Application.Run(new TrayIcon());          
        }
    }
}