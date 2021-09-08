using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> CNCIdAsync()
        {
            return await Task.FromResult(CNCId());
        }
        
        public dynamic CNCId()
        {
            uint[] cncid = new uint[4];

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdcncid(_handle, cncid);
            });

            var nr = new
            {
                method = "cnc_rdcncid",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://docs.ladder99.com/focas-api/misc/cnc_rdcncid.xml",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdcncid = new { }},
                response = new {cnc_rdcncid = new {cncid}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}