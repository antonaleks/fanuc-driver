﻿using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    /*
        y cnc_sysinfo;
        - cnc_sysinfo_ex;
        n cnc_sysconfig;    - HSSB only

        - cnc_rdsyssoft;
        - cnc_rdsyssoft2;
        - cnc_rdsyssoft3;
        - cnc_rdsyshard;
        - cnc_rdetherinfo;

        - cnc_rdproginfo;
        - cnc_rdprgnum;
        - cnc_exeprgname;
        y gcode
            y cnc_rdseqnum;
            y cnc_rdblkcount;
            y cnc_rdexecprog;
        - cnc_rdrepeatval;
        - cnc_rdrepeatval_ext;

        y cnc_rdparam (6711, 6712 and 6713);

        - cnc_alarm;
        - cnc_alarm2;
        - cnc_rdalminfo;
        y cnc_rdalmmsg;
        y cnc_rdalmmsg2;
        n cnc_getdtailerr;  - this is unnecessary.  Return status of Focas functions is already trapped within original Focas response.
        y cnc_statinfo;
        - cnc_statinfo2;
        - cnc_rdopnlsgnl (4, 5);
        - cnc_rdopnlsgn (3);
     */
    public class UseCase01 : FanucCollector2
    {
        public UseCase01(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            await Apply(typeof(fanuc.veneers.CNCId), "cnc_id");
            
            await Apply(typeof(fanuc.veneers.Alarms), "alarms");
            
            await Apply(typeof(fanuc.veneers.Alarms2), "alarms2");
            
            await Apply(typeof(fanuc.veneers.OpMsgs), "message1");
            
            //await Apply(typeof(fanuc.veneers.OpMsgs), "message2");

            await Apply(typeof(fanuc.veneers.RdPmcTitle), "pmc_title");
            
            await Apply(typeof(fanuc.veneers.RdPmcRngByte), "y0003");
            
            await Apply(typeof(fanuc.veneers.RdPmcRngByte), "y0008");
            

        }
        
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.SysInfo), "sys_info");
            
            await Apply(typeof(fanuc.veneers.StatInfoText), "stat_info");

            await Apply(typeof(fanuc.veneers.Figures), "figures");

            await Apply(typeof(fanuc.veneers.GCodeBlocks), "gcode_blocks");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "6711");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "6712");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "6713");
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            
            await Apply(typeof(fanuc.veneers.RdDynamic2_1), "axis_data");
            
            await Apply(typeof(fanuc.veneers.RdActs2), "spindle_data");

        }
        
        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        public override async Task CollectRootAsync()
        {
            await SetNativeAndPeel("cnc_id", await platform.CNCIdAsync());
            
            await SetNativeAndPeel("alarms", await platform.RdAlmMsgAllAsync(10,20));
            await SetNativeAndPeel("alarms2", await platform.RdAlmMsg2AllAsync(10,20));
                    
            await SetNativeAndPeel("message1", await platform.RdOpMsgAsync(0, 6+256));
            
            await SetNativeAndPeel("message1", await platform.RdOpMsgAsync(0, 6+256));
            
            await Apply(typeof(fanuc.veneers.RdAxisname), "axis_names");

            //await SetNativeAndPeel("message2", await platform.RdOpMsgAsync(1, 6+256));
            
            //var a = await platform.RdOpMsg1_15_15i_Async();
            //var b = await platform.RdOpMsg2_15_15i_Async();
            //var c = await platform.RdOpMsg3_15_15i_Async();
            //var d = await platform.RdOpMsg4_15_15i_Async();
            //var e = await platform.RdOpMsgMacro_15_15i_Async();
            //var f = await platform.RdOpMsgAll_15_15i_Async();
            //var g = await platform.RdOpMsg1_16i_18iW_Async();
            //var h = await platform.RdOpMsg2_16i_18iW_Async();
            //var i = await platform.RdOpMsg3_16i_18iW_Async();
            //var j = await platform.RdOpMsg4_16i_18iW_Async();
            //var k = await platform.RdOpMsgAll_16i_18iW_Async();
            //var l = await platform.RdOpMsg1_16_18_21_16i_18i_21i_0i_30i_PowerMatei_PMiA_Async();

            await SetNativeAndPeel("pmc_title", await platform.RdPmcTitleAsync());
            
            await SetNativeAndPeel("y0003", await platform.RdPmcRngYByteAsync(3));
            
            await SetNativeAndPeel("y0008", await platform.RdPmcRngYByteAsync(8));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await SetNativeAndPeel("sys_info", await platform.SysInfoAsync());
                        
            await SetNativeAndPeel("stat_info", await platform.StatInfoAsync());
            
            await SetNativeAndPeel("figures", await platform.GetFigureAsync(0, 32));
            
            await Peel("gcode_blocks",
                await SetNative("blkcount", await platform.RdBlkCountAsync()),
                await SetNative("actpt", await platform.RdActPtAsync()),
                await SetNative("execprog", await platform.RdExecProgAsync(256)));
            
            await SetNativeAndPeel("6711", await platform.RdParamDoubleWordNoAxisAsync(6711));
            
            await SetNativeAndPeel("6712", await platform.RdParamDoubleWordNoAxisAsync(6712));
            
            await SetNativeAndPeel("6713", await platform.RdParamDoubleWordNoAxisAsync(6713));
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            await Peel("axis_data",
                await SetNative("axis_dynamic", await platform.RdDynamic2Async(current_axis, 44, 2)), 
                Get("figures"), 
                current_axis - 1);
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}