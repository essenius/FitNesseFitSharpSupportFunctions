// Copyright 2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

#if NET5_0
namespace SupportFunctions.Model
{
    internal class LegendCursor
    {
        private readonly float _startXPosition;

        public LegendCursor(float maxWidth, float textHeight)
        {
            TextHeight = textHeight;
            MaxWidth = maxWidth;
            X = ShortMargin;
            _startXPosition = X;
            Y = X;
            MaxX = X;
        }
        public float GeometryX => (X + LineEndX) / 2;
        public float LineEndX => X + LineWidth;
        private float LineWidth {get; set; }
        public float LineY => Y + LineYOffset;
        private float LineYOffset { get; set; }
        private float Margin { get; set; }
        private float MaxWidth { get; }
        public float MaxX { get; private set; }
        public float MaxY => Y + TextHeight + ShortMargin;
        private float ShortMargin { get; set; }

        private float _textHeight;

        private float TextHeight
        {
            get => _textHeight;
            set
            {
                _textHeight = value;
                LineWidth = value;
                LineYOffset = value / 2f;
                TextYOffset = value * 0.8f;
                ShortMargin = value / 3f;
                Margin = ShortMargin * 2f;
            }
        }
        public float TextStartX => LineEndX + ShortMargin;
        public float TextWidth { get; set; }
        public float TextY => Y + TextYOffset;
        private float TextYOffset { get; set; }
        public float X { get; private set; }
        private float Y { get; set; }

        public void CheckNextLine()
        {
            if (X > MaxWidth)
            {
                X = _startXPosition;
                MaxX = MaxWidth;
                Y = MaxY;
            }
            else
            {
                UpdateMaxX();
            }
        }

        public void NextSeries()
        {
            X = TextStartX + Margin + TextWidth;
            UpdateMaxX();
        }

        private void UpdateMaxX()
        {
            if (MaxX < X)
            {
                MaxX = X;
            }
        }
    }
}
#endif