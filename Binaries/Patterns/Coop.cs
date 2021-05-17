using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueBot;
using LeagueBot.Patterns;
using LeagueBot.Game.Enums;
using LeagueBot.Game.Misc;
 
namespace LeagueBot
{
    public class Coop : PatternScript
    {
        long time = DateTimeOffset.Now.ToUnixTimeSeconds();
        Random rnd = new Random();
        int level = 0;
        private Point CastTargetPoint
        {
            get;
            set;
        }
        private int AllyIndex
        {
            get;
            set;
        }
        private int lastLane = 0;
        private bool safe = true;
 
        private Item[] Items = new Item[]
        {
            new Item("Long Sword",350),
            //Inmortal receipt
            new Item("Noonquiver",1300),
 
            new Item("Vampiric",900),
            new Item("Berserker", 1100),
            new Item("Cloak of Agility",600),
            //Inmortal
            new Item("Immortal Shieldbow",600),
 
 
            //BORK receipt
            new Item("Vampiric", 900),
            new Item("Recurve", 1000),
            new Item("Blade of the", 1300),
 
            //Runaan's Hurricane receipt
            new Item("Zeal", 1050),
            new Item("Dagger", 300),
			new Item("Dagger", 300),
            //Runaan's
			new Item("Hurricane", 950),
        };
 
        public override bool ThrowException
        {
            get
            {
                return false;
            }
        }
 
        public override void Execute()
        {
            bot.log("Waiting for league of legends process...");
 
            bot.waitProcessOpen(Constants.GameProcessName);
 
            bot.waitUntilProcessBounds(Constants.GameProcessName, 1030, 797);
 
            bot.wait(200);
 
            bot.log("Waiting for game to load.");
 
            bot.bringProcessToFront(Constants.GameProcessName);
            bot.centerProcess(Constants.GameProcessName);
 
            game.waitUntilGameStart();
 
            bot.log("Game Started");
 
            bot.bringProcessToFront(Constants.GameProcessName);
            bot.centerProcess(Constants.GameProcessName);
 
            bot.wait(1000);
 
            if (game.getSide() == SideEnum.Blue)
            {
                CastTargetPoint = new Point(1084, 398);
                bot.log("We are blue side !");
            }
            else
            {
                CastTargetPoint = new Point(644, 761);
                bot.log("We are red side !");
            }
 
            game.player.upgradeSpellOnLevelUp();
 
            OnSpawnJoin();
 
            bot.log("Playing...");

            GameLoop();
 
            this.End();
        }
 
        private void BuyItems()
        {
            int golds = game.player.getGolds();
            game.shop.toogle();
            bot.wait(3000);
            foreach (Item item in Items)
            {
                if (item.Cost > golds)
                {
                    break;
                }
                if (!item.Buyed)
                {
                    bot.log("Looking to buy " + item.Name);
                    game.shop.searchItem(item.Name);
 
                    game.shop.buySearchedItem();
 
                    item.Buyed = true;
 
                    golds -= item.Cost;
                }
            }
 
            game.shop.toogle();
            MoveToLane();
            bot.wait(2000);
        }
 
        private void CheckBuyItems()
        {
            //if we bought items with this function in the last 150 seconds returns
            long time_ = DateTimeOffset.Now.ToUnixTimeSeconds();
            //bot.log("Porch Time:"+ (time_ - time).ToString());
            if(time_-time<150)
            {
                return;
            }
            time = DateTimeOffset.Now.ToUnixTimeSeconds();
 
 
 
 
            int golds = game.player.getGolds();
 
            foreach (Item item in Items)
            {
                if (item.Cost > golds && !item.Buyed) //if item cost  > then gold, and was not bought, break because cant purchase anything;
                {
                    break;
                }
 
                //if we passed the upper condition, it means that we can buy items
                if (!item.Buyed)
                {
                    bot.log("CheckItem: Looking to buy " + item.Name);
 
                    game.player.moveToSafePosition(lastLane); //moves to a safer position
                    bot.wait(8000);
 
                    game.player.recall(); //starts recalling
 
                    bot.wait(10000);
 
                    //wait untill mana is 
                    while(game.player.getManaPercent() != 1)
                    {
                        bot.log("Refilling HP and Mana");
                        bot.wait(2000);
                    }
                    OnSpawnJoin();
 
                    return;
                }
            }
 
 
        }
 
        private void GameLoop()
        {
            level = game.player.getLevel();
 
            bool isRecalling = false;
 
            bool hasPot = true;
 
            //MoveToLane();
 
            while (bot.isProcessOpen(Constants.GameProcessName))
            {
                bot.bringProcessToFront(Constants.GameProcessName);
 
                bot.centerProcess(Constants.GameProcessName);
 
                int newLevel = game.player.getLevel();
 
                if (newLevel != level)
                {
                    level = newLevel;
                    game.player.upgradeSpellOnLevelUp();
                }
 
 
                checkIfDead();
 
                if (isRecalling)
                {
                    game.player.moveToSafePosition(lastLane);
                    bot.wait(5000);
                    game.player.recall();
                    bot.wait(8500);
                    int time = 0;
                    while (game.player.getManaPercent() != 0.9 && time < 35000)
                    {
                        bot.log("Refilling HP and Mana");
                        bot.wait(2000);
                        time += 2000;
                    }
                    OnSpawnJoin();
                    isRecalling = false;
                    continue;
                }
 
                if(game.player.getHealthPercent() <= 0.70d && hasPot)
                {
                    game.player.usePot();
                    hasPot = false;
                }
 
                if (game.player.getManaPercent() <= 0.10d)
                {
                    isRecalling = true;
                    continue;
                }
 
                if (game.player.getHealthPercent() <= 0.20d)
                {
                    isRecalling = true;
                    continue;
                }
 
                BotLogicManager();
 
 
            }
        }
        public void followMinions()
        {
            bot.log("Looking for minions");
            game.player.moveToSafePosition(lastLane);
            while (!game.player.hasEnemyToAttack())
            {
                checkIfDead();
                //bot.log("Looking for minions");
                game.player.followNearbyMinion();
                bot.wait(rnd.Next(100, 300));
            }
        }
        public void BotLogicManager()
        {
            switch (State())
            {
                case 1:
                    //if no enemy nearby follow minions untill found enemies
                    followMinions();
 
                    break;
                case 2:
                    //if there is a champion and no tower
                   // bot.log("Attacking  champion.");
 
                    AttackChampion();
 
                    break;
                case 3:
                    //attacking minions
 
                    //bot.log("Attacking minions.");
                    AttackingMinions();
 
                    break;
                case 4:
                    //if there is a tower and no enemy champion then attack
                    //bot.log("Attacking towers.");
                    AttackTowers();
                    break;
 
                case 5:
                    fallBack();
                    bot.wait(2000);
                    break;
 
                default:
                    followMinions();
                    break;
            }
            CheckBuyItems();
        }
 
        public void fallBack()
        {
            game.player.moveToSafePosition(lastLane);
        }
 
        public void AttackingMinions()
        {
            int moveBackCounter = 0;
            while (game.player.areAllyMinionsNearby() && game.player.hasEnemyMinionToAttack() && !game.player.hasEnemyChampionToAttack())
            {
                game.player.autoAttackEnemyMinion();
                game.player.tryCastSpellOnTarget(2); // W
                game.player.tryCastSpellOnTarget(1); // Q
                checkIfDead();
                moveBackCounter++;
                if(moveBackCounter%7 == 0)
                {
                    game.player.moveBack();
                }
            }
        }
 
        public void AttackChampion()
        {
            while(game.player.areAllyMinionsNearby() && !game.player.areEnemyTowersNearby() && game.player.hasEnemyChampionToAttack())
            {
                game.player.autoAttackEnemyChampion();
                checkIfDead();
 
                game.player.tryCastSpellOnTarget(3,1);  //e
                game.player.tryCastSpellOnTarget(2,1); // W
 
                game.player.autoAttackEnemyChampion();
 
                game.player.tryCastSpellOnTarget(1,1); // Q
 
                if (game.player.tryCastSpellOnTarget(4,1))
                {
                    bot.wait(3500);//mf channelling her ult
                }; // ult 
 
                if (game.player.getHealthPercent() <= 0.30d)
                {
                    game.player.tryCastSpellOnTarget(5,1);//right summoner spell // my case Heal
                }
 
                game.player.tryCastSpellOnTarget(6,1); //left summoner spell // Ghost
 
                checkIfDead();
            }
 
            //bot.log("No champion to attack, or there is a tower, or no more allied minions");
        }
        public void AttackTowers()
        {
            game.player.autoAttackTower();
            bot.wait(1000);
            game.player.autoAttackTower();
            bot.wait(1000);
            game.player.autoAttackTower();
            bot.wait(1000);
 
            checkIfDead();
            game.player.moveBack();
            bot.wait(2000);
        }
 
        public void checkIfDead()
        {
            levelCheck();
            //bot.log("Checking if dead: " + game.player.dead() +" Health at "+ game.player.getHealthPercent()*100 + "%");
            bool dead = false;
            while (game.player.dead() || game.player.getHealthPercent()<=0.03d) //because game.player.dead() does not always retreat true in time if our player is dead
                //we check the current health as well
            {
                game.player.recall(); //just in case if we didnt die (because we check hp < 3%)
                bot.log("Ah shiet, we died...");
                if (!dead)
                {
                    dead = true;
                }
                while(game.player.getHealthPercent()!=1)
                {
                    bot.log("Waiting to spawn.");
                    bot.wait(3000);
                }
            }
 
            if (dead)
            {
                dead = false;
                OnDie();
            }
        }
 
        private void OnDie()
        {
            BuyItems();
        }
 
        private void OnSpawnJoin()
        {
            bot.wait(2000);
            BuyItems();
        }
 
        private void MoveToLane()
        {
            bot.wait(2000);
            int lane = rnd.Next(3);
            game.player.moveToLane(lane);
            lastLane = lane;
            bot.wait(15000);
        }
        private void OnRevive()
        {
            MoveToLane();
        }
 
        public void levelCheck()
        {
 
            int newLevel = game.player.getLevel();
 
            if (newLevel != level)
            {
                level = newLevel;
                game.player.upgradeSpellOnLevelUp();
            }
        }
 
 
 
        public int State()
        {
            /*
             Simple rules:
                if no minions, go back and w8 for them
                if there are minions attack them
                if there are tower attack them
                if there are champions attack them
                prioritize champions over minions whene there is no tower
             */
            //if there is a champion and a tower
 
            //if no ally minions are nearby
            if (!game.player.areAllyMinionsNearby())
            {
                //bot.log("No minions nearby.");
                return 1;
            }
 
            if (game.player.hasEnemyChampionToAttack() && !game.player.areEnemyTowersNearby())
            {
                //bot.log("Attacking champions. No tower.");
                return 2;
            }
 
            if (game.player.hasEnemyMinionToAttack())
            {
                ///bot.log("Attacking minions.");
                return 3;
            }
 
            if(!game.player.hasEnemyChampionToAttack() && game.player.areEnemyTowersNearby())
            {
                //bot.log("Attacking towers");
                return 4;
            }
 
 
 
            if(game.player.hasEnemyChampionToAttack() && game.player.areEnemyTowersNearby())
            {
                //bot.log("A champion and a tower detected. Backing off.");
                return 5;
            }
 
 
            return 0;
        }
 
        public override void End()
        {
            bot.executePattern("EndCoop");
            base.End();
        }
    }
}