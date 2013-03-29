using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kgrep {
    class ReadStdin : IHandleInput  {

            public ReadStdin() {
            }

            public string ReadLine() {
                return Console.ReadLine();
            }

            public void Close() {
            }
        }
}
