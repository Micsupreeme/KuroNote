namespace KuroNote
{
    public class KuroNoteAchievement
    {
        //meta
        public int achievementId;
        public string achievementName;
        public string achievementDesc;
        public bool achievementHide;
        public KuroNoteTheme rewardTheme;

        public KuroNoteAchievement(int _achievementId, string _achievementName, string _achievementDesc, bool _achievementHide, KuroNoteTheme _rewardTheme = null)
        {
            this.achievementId = _achievementId;
            this.achievementName = _achievementName;
            this.achievementDesc = _achievementDesc;
            this.achievementHide = _achievementHide;
            this.rewardTheme = _rewardTheme;
        }
    }
}
