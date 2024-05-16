using eDrawings.Interop.EModelViewControl;

namespace CadConversion
{
    public class EDrawingsHost : AxHost
    {
        public event Action<EModelViewControl>? ControlLoaded;
        private bool m_IsLoaded;

        public EDrawingsHost() : base("22945A69-1191-4DCF-9E6F-409BDE94D101")
        {
            m_IsLoaded = false;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!m_IsLoaded)
            {
                m_IsLoaded = true;
                if (GetOcx() is EModelViewControl ctrl)
                {
                    ControlLoaded?.Invoke(ctrl);
                }
            }
        }
    }
}