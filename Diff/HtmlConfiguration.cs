namespace BlazormeDiff
{
    public class HtmlConfiguration
    {
        // "line-by-line" or "side-by-side", default is "line-by-line"
        internal const string SideBySide = "side-by-side";
        internal const string LineByLine = "line-by-line";
        public string OutputFormat { get; set; }

        // default is true
        public bool DrawFileList { get; set; }

        // show differences level in each line: word or char, default is word
        internal const string Word = "word";
        internal const string Char = "char";
        public string DiffStyle { get; set; }

        // matching level: 'lines' for matching lines, 'words' for matching lines and words or 'none', default is none
        internal const string Lines = "lines";
        internal const string Words = "words";
        internal const string None = "none";
        public string Matching { get; set; } 
    }
}
