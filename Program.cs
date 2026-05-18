using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;

namespace AP_TrapLink_Tester
{
    internal class Program
    {
        // Connection info, change this to change the slot and server that's being connected to.
        static string server = "localhost:62746";
        static string slot = "Trap Tester";
        static string game = "Freedom Planet 2";
        static string password = null;

        static void Main(string[] args)
        {
            // Create the session and try to log in to it.
            ArchipelagoSession session = ArchipelagoSessionFactory.CreateSession(server);
            LoginResult connectionResult = session.TryConnectAndLogin(game, slot, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, null, null, null, password, true);

            // If we fail to connect, then throw an exception with the errors.
            if (!connectionResult.Successful)
            {
                LoginFailure connectionFailure = (LoginFailure)connectionResult;
                throw new Exception(String.Join("\r\n", connectionFailure.Errors));
            }

            // Add the TrapLink tag.
            session.ConnectionInfo.UpdateConnectionOptions([.. session.ConnectionInfo.Tags, .. new string[1] { "TrapLink" }]);

            // Run the packet function.
            Packet(session);
        }

        static void Packet(ArchipelagoSession session)
        {
            // Grab the user's input, clearing the console to keep it plain,
            Console.Clear();
            Console.WriteLine("Enter trap name to send:");
            string trapName = Console.ReadLine();

            // Send a TrapLink packet to the server with the given trap name.
            BouncePacket packet = new()
            {
                Tags = ["TrapLink"],
                Data = new()
                        {
                            { "time", (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds },
                            { "source", session.Players.GetPlayerName(session.ConnectionInfo.Slot) },
                            { "trap_name", trapName }
                        }
            };
            session.Socket.SendPacket(packet);

            // Rerun this function so we can infinitely loop.
            Packet(session);
        }
    }
}
