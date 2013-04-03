using System;

namespace kgrep {
    class ReadStdin : IHandleInput  {

        public string ReadLine() {
                return Console.ReadLine();
            }

            public void Close() {
            }
        }
}
