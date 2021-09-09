namespace KuroNote
{
    public class KuroNoteRank
    {
        //meta
        public int rankId;
        public string rankName;

        //ap (arbitrary points)
        public int apToNext;

        //colors
        public string textBrush;

        public KuroNoteRank(int _rankId, string _rankName, int _apToNext, string _textBrush)
        {
            this.rankId = _rankId;
            this.rankName = _rankName;
            this.apToNext = _apToNext;
            this.textBrush = _textBrush;
        }
    }
}
