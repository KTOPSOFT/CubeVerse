using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public abstract class BasicMathOperationNode<T> : BasicMathOperationNode<T, T, T> { }

    public abstract class BasicMathOperationNode<T1, T2, TOut> : ProcessorNode
    {
        private InPort aIn = null;
        private InPort bIn = null;
        private OutPort resultOut = null;

        public override void OnLoadInputPorts()
        {
            aIn = AddIn<T1>("A");
            aIn.IsConnectionRequired = true;
            
            bIn = AddIn<T2>("B");
            bIn.IsConnectionRequired = true;
            
            base.OnLoadInputPorts();
        }

        public override void OnLoadOutputPorts()
        {
            resultOut = AddOut<TOut>("Result");
        }

        protected abstract TOut Calculate(T1 a, T2 b);

        protected override void OnProcess()
        {
            var a = aIn.Get<T1>();
            var b = bIn.Get<T2>();

            var result = Calculate(a, b);
            
            resultOut.Set(() => result);
        }
    }
}