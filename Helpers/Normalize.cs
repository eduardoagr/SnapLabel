namespace SnapLabel.Helpers {
    public static class Normalize {
        private static readonly Regex _whitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        public static string NormalizeStrings(string? input) {

            if(string.IsNullOrWhiteSpace(input)) {
                return string.Empty;
            }

            var trimmed = input.Trim();
            var collapsed = _whitespaceRegex.Replace(trimmed, " ");

            var words = collapsed.Split(' ');
            for(int i = 0; i < words.Length; i++) {
                if(words[i].Length > 0) {
                    var first = char.ToUpperInvariant(words[i][0]);
                    var rest = words[i].Substring(1).ToLowerInvariant();
                    words[i] = first + rest;
                }
            }

            return string.Join(" ", words);
        }
    }
}