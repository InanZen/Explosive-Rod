using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Hooks;
using TShockAPI;

namespace ExplosiveRod
{

    [APIVersion(1, 12)]
    public class ExplosiveRod: TerrariaPlugin
    {        
        private static bool[] cannonPlayers = new bool[256];
        private static List<Projectile> projectileList = new List<Projectile>();
        public override string Name
        {
            get { return "Explosive rod"; }
        }
        public override string Author
        {
            get { return "by InanZen"; }
        }
        public override string Description
        {
            get { return "Shoots Explosives from flamelash"; }
        }
        public override Version Version
        {
            get { return new Version("1.8"); }
        }
        public override void Initialize()
        {     
            GameHooks.Update += OnUpdate;
            GetDataHandlers.NewProjectile += OnProjectile;
            ServerHooks.Leave += OnLeave;

            Commands.ChatCommands.Add(new Command("explosiverod", cannonCommand, "cannon"));
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Update -= OnUpdate;
                GetDataHandlers.NewProjectile += OnProjectile;
                ServerHooks.Leave -= OnLeave;
            }
            base.Dispose(disposing);
        }
        public ExplosiveRod(Main game)
            : base(game)
        {
            Order = 5;
        }
        void OnLeave(int who)
        {
            cannonPlayers[who] = false;
        }
        public void OnUpdate()
        {
            try
            {
                for (int i = projectileList.Count - 1; i >= 0; i--)
                {
                    if (!Main.player[projectileList[i].owner].channel || !projectileList[i].active)
                    {
                        int x = (int)(projectileList[i].position.X / 16);
                        int y = (int)(projectileList[i].position.Y / 16);
                        generateExplosives(x, y);
                        Main.tile[x + 1, y].active = true;
                        Main.tile[x + 1, y].wire = true;
                        Main.tile[x + 1, y].type = 0;
                        TShockAPI.TSPlayer.All.SendTileSquare(x, y, 4);
                        WorldGen.TripWire(x + 1, y);
                        Main.tile[x, y].wire = false;
                        Main.tile[x + 1, y].wire = false;
                        TShockAPI.TSPlayer.All.SendTileSquare(x, y, 4);                    
                        projectileList[i].active = false;
                        NetMessage.SendData((int)PacketTypes.ProjectileDestroy, -1, -1, "", projectileList[i].identity);
                        projectileList.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex) { Log.ConsoleError(ex.ToString()); }


        }
        void OnProjectile(Object sender, GetDataHandlers.NewProjectileEventArgs args)
        {         
            if (args.Owner < 255)
            {
                var projectile = Main.projectile[args.Identity];
                if (projectile.active && projectile.type == 34 && cannonPlayers[args.Owner] && Main.player[args.Owner].channel && !projectileList.Contains(projectile))
                {
                    projectileList.Add(projectile);
                }
            }
        }
        public void generateExplosives(float x, float y)
        {
            generateExplosives((int)(x / 16), (int)(y / 16));              
        }

        public void generateExplosives(int x, int y)
        {
            Main.tile[x, y].active = true;
            Main.tile[x, y].type = 141;
            Main.tile[x, y].frameX = 0;
            Main.tile[x, y].frameY = 18;
            Main.tile[x, y].wire = true;
        }
      
        public void cannonCommand(CommandArgs args)
        {
            int plyID = args.Player.Index;
            if (!cannonPlayers[plyID])
            {
                cannonPlayers[plyID] = true;
                args.Player.SendMessage("Cannon now ENABLED", Color.DarkGreen);
            }
            else
            {
                cannonPlayers[plyID] = false;
                args.Player.SendMessage("Cannon now DISABLED", Color.DarkRed);
            }
        }
    }
}
