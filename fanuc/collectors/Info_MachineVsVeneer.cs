﻿using System;
using System.Collections.Generic;
using System.Linq;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Info_MachineVsVeneer : FanucCollector
    {
        public Info_MachineVsVeneer(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = ((FanucMachine)_machine).Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    
                    dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    System.Threading.Thread.Sleep(_sweepMs);
                }
            }
        }

        public override void Collect()
        {
            dynamic connect = ((FanucMachine)_machine).Platform.Connect();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic info_machine = ((FanucMachine)_machine).Platform.SysInfo();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(JObject.FromObject(info_machine).ToString());
                dynamic info_veneer = _machine.PeelVeneer("sys_info", info_machine);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(JObject.FromObject(info_veneer.veneer.LastValue).ToString());

                dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();

                LastSuccess = connect.success;
            }
        }
    }
}