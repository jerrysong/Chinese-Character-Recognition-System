using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    public class Character
    {        
        private List<Stroke> strokeList;

        public int PointsCount
        {
            get; 
            set;
        }
        public Radical Radical
        {
            get;
            set;
        }
        public char Font
        {
            get;
            set;
        }

        public Character()
        {
            strokeList = new List<Stroke>();
            Radical = Radical.None;
        }

        // Deep copy
        public Character(Character c)
        {
            List<Stroke> strokeList = new List<Stroke>();
            for (int i = 0; i < c.StrokeCount(); i++)
            {
                strokeList.Add(c[i]);
            }
            PointsCount = c.PointsCount;
            Radical = c.Radical;
            Font = c.Font;
        }

        // Soft copy
        public Character(List<Stroke> strokeList)
        {
            this.strokeList = strokeList;
            Radical = Radical.None;
        }

        public Stroke this[int i]
        {
            get { return strokeList[i]; }
            set { strokeList[i] = value; }
        }

        public void Add(Stroke s)
        {
            strokeList.Add(s);
        }

        public int StrokeCount()
        {
            return strokeList.Count;
        }

        public Stroke Last()
        {
            return strokeList.Last();
        }

        public Stroke First()
        {
            return strokeList.First();
        }

        public List<Stroke> Content()
        {
            return strokeList;
        }

        public void Insert(int i, Stroke insertStroke)
        {
            strokeList.Insert(i, insertStroke);
        }

        public void RemoveAt(int i)
        {
            strokeList.RemoveAt(i);
        }
    }
}
