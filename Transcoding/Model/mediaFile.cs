using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transcoding.Model
{
    class MediaFile
    {
        private string name;
        private string path;
        private double progress;

        public string Name { get => name; set => name = value; }
        public string Path { get => path; set => path = value; }
        public double Progress
        {
            get { return progress; }
            set
            {
                if (value <= 100)
                    progress = value;
                else
                    throw new Exception("invalid progress");
            }
        }
    }
}
