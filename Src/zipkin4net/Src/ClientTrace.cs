using System;

namespace zipkin4net
{
    public class ClientTrace : BaseStandardTrace, IDisposable
    {
        public ClientTrace(string serviceName, string rpc)
        {
            if (Trace.Current != null)
            {
                Trace = Trace.Current.Child();
            }

            Trace.Record(Annotations.ClientSend());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ClientRecv());
        }
    }
}
