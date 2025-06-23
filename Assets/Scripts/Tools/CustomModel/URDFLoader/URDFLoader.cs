using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URDFLoader
{
    public class URDFLoader
    {
        private static URDFLoader _Instance;

        public URDFLoader Instance 
        {
            get 
            {
                _Instance = new URDFLoader();
                return _Instance;
            }
        }

        public void Init()
        {
            
        }
    }
}
