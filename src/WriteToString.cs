﻿using System.Text;

namespace kgrep {
    public class WriteToString : IHandleOutput {
        private StringBuilder _sb = new StringBuilder();

        public void Write(string line) {
            _sb.Append(line);
            if (!line.EndsWith("\n")) _sb.Append("\n");
        }

        public string Close() {
            return _sb.ToString();
        }
    }
}