using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace TEST_lire_ecrire_image
{
    public class Pixel
    {
        private int[] tabPixels;

        public Pixel(int[] tabPixels) //en paramètre : le tableau comportant les trois indices de couleur de chaque pixel (ou plus si l'on définit une image)

        {
            this.tabPixels = new int[3];
            for (int i = 0; i < tabPixels.Length; i++)
            {
                this.tabPixels[i] = tabPixels[i];
            }
        }

        public int[] Pixels
        {
            get { return tabPixels; }  //accès en lecture
            set { tabPixels = value; } //accès en écriture
        }
    }
}