using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    public class KuroNoteCustomTheme
    {
        //meta
        public int themeId;
        public string themeName;
        public bool hasImage;

        //colours
        public string bgBrush;
        public double imgBrushOpacity;
        public string solidBrush;
        public string menuBrush;
        public string statusBrush;
        public string textBrush;

        //font
        public string fontFamily;
        public short fontSize;
        public FontWeight fontWeight;
        public FontStyle fontStyle;

        //deserialise constructor
        [JsonConstructor]
        public KuroNoteCustomTheme(int _themeId, string _themeName, bool _hasImage,
                            string _bgBrush, double _imgBrushOpacity, string _solidBrush,
                            string _menuBrush, string _statusBrush, string _textBrush,
                            string _fontFamily, short _fontSize, FontWeight _fontWeight, FontStyle _fontStyle)
        {

            this.themeId = _themeId;
            this.themeName = _themeName;
            this.hasImage = _hasImage;
            this.hasImage = _hasImage;
            this.bgBrush = _bgBrush;
            this.imgBrushOpacity = _imgBrushOpacity;
            this.solidBrush = _solidBrush;
            this.menuBrush = _menuBrush;
            this.statusBrush = _statusBrush;
            this.textBrush = _textBrush;
            this.fontFamily = _fontFamily;
            this.fontSize = _fontSize;
            this.fontWeight = _fontWeight;
            this.fontStyle = _fontStyle;
        }
    }
}
