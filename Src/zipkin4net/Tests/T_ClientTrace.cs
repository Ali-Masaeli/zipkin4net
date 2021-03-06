using NUnit.Framework;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Annotation;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_ClientTrace
    {
        private Mock<IRecordDispatcher> dispatcher;
        private const string serviceName = "service1";
        private const string rpc = "rpc";

        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
            dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);
        }

        [Test]
        public void ShouldNotSetCurrentTrace()
        {
            Trace.Current = null;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                Assert.IsNull(client.Trace);
            }
        }

        [Test]
        public void ShouldCallChildWhenCurrentTraceNotNull()
        {
            var trace = Trace.Create();
            Trace.Current = trace;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                Assert.AreEqual(trace.CurrentSpan.SpanId, client.Trace.CurrentSpan.ParentSpanId);
                Assert.AreEqual(trace.CurrentSpan.TraceId, client.Trace.CurrentSpan.TraceId);
            }
        }

        [Test]
        public void ShouldLogClientAnnotations()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            // Act
            var trace = Trace.Create();
            trace.ForceSampled();
            Trace.Current = trace;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                // Assert
                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is ClientSend)));

                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is ServiceName
                            && ((ServiceName)m.Annotation).Service == serviceName)));

                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is Rpc
                            && ((Rpc)m.Annotation).Name == rpc)));
            }

            // Assert
            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is ClientRecv)));
        }
    }
}