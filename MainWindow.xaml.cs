// TODO: Look in to how EnergyLink works and add a way to test that.
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Archipealgo_Link_Tester
{
    public partial class MainWindow : Window
    {
        // Set up the session and DeathLink variables.
        public static ArchipelagoSession session;
        public static DeathLinkService deathLink;

        public MainWindow()
        {
            InitializeComponent();

            // Load the saved settings.
            TextBox_Connection_Server.Text = Properties.Settings.Default.Server;
            TextBox_Connection_Slot.Text = Properties.Settings.Default.Slot;
            TextBox_Connection_Password.Text = Properties.Settings.Default.Password;
        }

        // Update the settings.
        private void ServerAddress_Update(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Server = TextBox_Connection_Server.Text;
            Properties.Settings.Default.Save();
        }
        private void Slot_Update(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Slot = TextBox_Connection_Slot.Text;
            Properties.Settings.Default.Save();
        }
        private void Password_Update(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Password = TextBox_Connection_Password.Text;
            Properties.Settings.Default.Save();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            // Create the session and try to log in to it.
            session = ArchipelagoSessionFactory.CreateSession(Properties.Settings.Default.Server);
            LoginResult connectionResult = session.TryConnectAndLogin("Link Tester", Properties.Settings.Default.Slot, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, null, null, null, Properties.Settings.Default.Password, true);

            // If we fail to connect, then display the errors and return.
            if (!connectionResult.Successful)
            {
                HandyControl.Controls.MessageBox.Show(String.Join("\r\n", ((LoginFailure)connectionResult).Errors),
                                                      "Connection Failed...",
                                                      MessageBoxButton.OK,
                                                      MessageBoxImage.Error);
                return;
            }

            // Create the DeathLink service.
            deathLink = session.CreateDeathLinkService();

            // Set out tags.
            session.ConnectionInfo.UpdateConnectionOptions([.. session.ConnectionInfo.Tags, .. new string[4] { "TrapLink", "DeathLink", "RingLink", "SharedDamage" }]);

            // Enable the other tabs and disable the connect button.
            Button_Connection_Connect.IsEnabled = false;
            Tab_TrapLink.IsEnabled = true;
            Tab_DeathLink.IsEnabled = true;
            Tab_RingLink.IsEnabled = true;
            Tab_DamageLink.IsEnabled = true;

            // Change the window title to show our connection.
            Title = $"Connected to {Properties.Settings.Default.Server} as {Properties.Settings.Default.Slot}.";
        }

        // Send a TrapLink packet using the inputted text as the trap name.
        // TODO: Maybe not do anything if the trap name textbox is left empty?
        private void TrapLink(object sender, RoutedEventArgs e)
        {
            BouncePacket packet = new()
            {
                Tags = ["TrapLink"],
                Data = new()
                {
                    { "time", (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds },
                    { "source", session.Players.GetPlayerName(session.ConnectionInfo.Slot) },
                    { "trap_name", TextBox_TrapLink_TrapName.Text }
                }
            };
            session.Socket.SendPacket(packet);
        }

        // Enable or disable the DeathLink cause textbox depending on the state of the null checkbox.
        private void DeathLinkNullToggle(object sender, RoutedEventArgs e)
        {
            if (CheckBox_DeathLink_Null.IsChecked == true)
                TextBox_DeathLink_Cause.IsEnabled = false;
            else
                TextBox_DeathLink_Cause.IsEnabled = true;
        }

        // Send a DeathLink, either with the inputted cause or a null cause depending on the state of the null checkbox.
        private void DeathLink(object sender, RoutedEventArgs e)
        {
            if (CheckBox_DeathLink_Null.IsChecked == false)
                deathLink.SendDeathLink(new DeathLink(session.Players.GetPlayerName(session.ConnectionInfo.Slot), TextBox_DeathLink_Cause.Text));
            else
                deathLink.SendDeathLink(new DeathLink(session.Players.GetPlayerName(session.ConnectionInfo.Slot), null));
        }

        // Send a RingLink packet of the specified amount.
        private void RingLink(object sender, RoutedEventArgs e)
        {
            BouncePacket packet = new()
            {
                Tags = ["RingLink"],
                Data = new()
                {
                    { "time", (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds },
                    { "source", session.ConnectionInfo.Slot },
                    { "amount", Numeric_RingLink_Count.Value }
                }
            };
            session.Socket.SendPacket(packet);
        }

        // Send a DamageLink packet of the specified amount.
        private void DamageLink(object sender, RoutedEventArgs e)
        {
            BouncePacket packet = new()
            {
                Tags = ["SharedDamage"],
                Data = new()
                {
                    { "time", (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds },
                    { "source", session.Players.GetPlayerName(session.ConnectionInfo.Slot) },
                    { "uuid", Guid.NewGuid() },
                    { "damage_points", Numeric_DamageLink_Count.Value }
                }
            };
            session.Socket.SendPacket(packet);
        }
    }
}
