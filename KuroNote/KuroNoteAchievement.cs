using System;
using System.Collections.Generic;
using System.Text;

namespace KuroNote
{
    public class KuroNoteAchievement
    {
        //meta
        public int achievementId;
        public string achievementName;
        public string achievementDesc;
        public KuroNoteTheme rewardTheme;

        public KuroNoteAchievement(int _achievementId, string _achievementName, string _achievementDesc, KuroNoteTheme _rewardTheme = null)
        {
            this.achievementId = _achievementId;
            this.achievementName = _achievementName;
            this.achievementDesc = _achievementDesc;
            this.rewardTheme = _rewardTheme;
        }
    }
}
