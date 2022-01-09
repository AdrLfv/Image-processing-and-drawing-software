using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace TEST_lire_ecrire_image
{
    public class MonImage
    {
        private int hauteurImage;
        private int tailleFichier;
        private int largeurImage;
        private string typeImage;
        private Pixel[,] matImage;
        private byte[] tabFichier;

        public MonImage(string fichier)
        {

            tabFichier = File.ReadAllBytes(fichier);
            if (tabFichier[0] == 66 && tabFichier[1] == 77) { typeImage = ".bmp"; }
            else { typeImage = "autre"; }

            byte[] tabBytes = { tabFichier[2], tabFichier[3], tabFichier[4], tabFichier[5] };
            tailleFichier = Convertir_Endian_To_Int(tabBytes);
            tabBytes = new byte[2] { tabFichier[18], tabFichier[19] };
            largeurImage = Convertir_Endian_To_Int(tabBytes);
            tabBytes = new byte[2] { tabFichier[22], tabFichier[23] };
            hauteurImage = Convertir_Endian_To_Int(tabBytes);
            tabBytes = new byte[2] { tabFichier[28], tabFichier[29] };
            matImage = new Pixel[hauteurImage, largeurImage]; //création d'une matrice de pixels comportant une hauteur et largeur (notre image)
            int k = 54;

            //calcul du padding
            int taillePadding = (((tabFichier.Length - 54) / hauteurImage) - largeurImage * 3) / 3;
            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    int[] tabOctets = { tabFichier[k], tabFichier[k + 1], tabFichier[k + 2] }; //on recopie les octets du fichier dans un tableau
                    matImage[i, j] = new Pixel(tabOctets); //on crée des pixels à partir de ces tableaux, on les places dans une matrice

                    k = k + 3;
                }

                k += taillePadding;

            }
        }

        public MonImage(int hauteurImage, int largeurImage)
        {
            matImage = new Pixel[hauteurImage, largeurImage];
            this.largeurImage = largeurImage;
            this.hauteurImage = hauteurImage;
            tabFichier = new byte[largeurImage * hauteurImage * 3 + 54];
        }

        public Pixel[,] MatImage
        {
            get { return (matImage); }
            set { matImage = value; }
        }

        public int TailleFichier
        {
            get { return (tailleFichier); }
        }

        public int Largeur
        {
            get { return largeurImage; }
        }

        public int Hauteur
        {
            get { return hauteurImage; }
        }

        public byte[] Fichier
        {
            get { return tabFichier; }
        }
        /// <summary>
        /// fonction qui modofie le header d'une image
        /// </summary>
        /// <param name="hauteurImage"></param>
        /// <param name="largeurImage"></param>
        /// <param name="unFichier"></param>
        /// <param name="taillePadding"></param>
        /// <returns>retourne un tableau de byte qui contient le header de l'image</returns>
        public byte[] Header(int hauteurImage, int largeurImage, byte[] unFichier, int taillePadding)
        {
            //Format de fichier
            unFichier[0] = 66;
            unFichier[1] = 77;
            //=================
            //Taille du fichier en octets
            for (int i = 0; i < 4; i++)
            {
                unFichier[2 + i] = Convertir_Int_To_Endian((largeurImage * 3 + taillePadding) * hauteurImage + 54)[i];
            }
            //==========================
            //Offset du fichier
            unFichier[10] = 54;
            //==========================
            //Taille de la zone Bitmap info
            unFichier[14] = 40;
            //=========================
            //Largeur de l'image
            for (int i = 18; i < 20; i++)
            {
                unFichier[i] = Convertir_Int_To_Endian(largeurImage)[i - 18];
            }
            //unFichier0[19] = 2;
            //=========================
            //Hauteur de l'image
            for (int i = 22; i < 24; i++)
            {
                unFichier[i] = Convertir_Int_To_Endian(hauteurImage)[i - 22];
            }
            //unFichier0[23] = 2;
            //=========================
            //Nombre de plans
            unFichier[26] = 1;
            //=========================
            //Bits par pixel
            unFichier[28] = 24;
            //=========================
            //Taille de l'image en octets
            for (int i = 34; i < 38; i++)
            {
                unFichier[i] = Convertir_Int_To_Endian(largeurImage * hauteurImage * 3)[i - 34];
            }
            //unFichier0[36] = 12;
            //=========================
            return unFichier;
        }
        /// <summary>
        /// fonction qui crée un fichier bmp à partir d'une matrice de pixel
        /// </summary>
        /// <param name="path">chemin du fichier créé</param>
        public void From_Image_To_File(string path)
        {
            int compteur = Convert.ToInt32(Math.IEEERemainder(matImage.GetLength(1), 4));
            if (compteur < 0)
            {
                compteur += 4;
            }
            byte[] monFichierFinal = new byte[matImage.GetLength(0) * (matImage.GetLength(1) + compteur) * 3 + 54];
            monFichierFinal = Header(matImage.GetLength(0), matImage.GetLength(1), monFichierFinal, compteur);
            int k = 54;
            int[] tab = { 0, 0, 0 };
            Pixel monPixel = new Pixel(tab);
            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    if (matImage[i, j] == null)
                    {
                        matImage[i, j] = monPixel;
                    }
                    for (int o = 0; o < matImage[i, j].Pixels.Length; o++)
                    {
                        monFichierFinal[k] = Convert.ToByte(matImage[i, j].Pixels[o]);
                        k++;
                    }
                }
                for (int l = 0; l < compteur; l++)
                {
                    monFichierFinal[k + l] = 0;
                }
                k += compteur;
            }
            File.WriteAllBytes(path, monFichierFinal);
        }
        /// <summary>
        /// fonction qui convertit le format little endian en int
        /// </summary>
        /// <param name="tab"></param>
        /// <returns>retourne l'entier convertit</returns>
        public int Convertir_Endian_To_Int(byte[] tab)
        {
            int result = 0;

            for (int i = 0; i < tab.Length; i++)
            {
                result += tab[i] * Convert.ToInt32(Math.Pow(256, i));
            }
            return result;
        }
        /// <summary>
        /// fonction qui convertit le format int en little endian
        /// </summary>
        /// <param name="valeur"></param>
        /// <returns>retourne une tableau de byte représentant le format little endian</returns>
        public byte[] Convertir_Int_To_Endian(int valeur)
        {
            byte[] tab = new byte[4];
            int somme = 0;
            for (int i = tab.Length - 1; i >= 0; i--)
            {
                tab[i] = Convert.ToByte((valeur - somme) / Convert.ToInt32(Math.Pow(256, i)));
                somme += tab[i] * Convert.ToInt32(Math.Pow(256, i));
            }
            return tab;
        }
        //public void GenererNuance()
        //{
        //    /*
        //    int a = 0;
        //    int b = 120;
        //    int c = 255;
        //    for (int i = 0; i < matImage.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < matImage.GetLength(1); j++)
        //        {
        //            int[] tabIndicesCouleurs = { a, b, c };
        //            matImage[i, j] = new Pixel(tabIndicesCouleurs);
        //            a += 1;
        //            b += 1;
        //            c -= 1;
        //            if (a > 255) { a = 0; }
        //            if (b > 255) { b = 120; }
        //            if (c < 0) { c = 255; }
        //        }
        //    }*/
        //    int i = 0;
        //    int colonne = matImage.GetLength(1) - 1;
        //    int j = colonne;

        //    for (int compteur = 0; compteur < matImage.Length; compteur++)
        //    {
        //        bool aCroiss = true;
        //        bool cCroiss = false;
        //        bool bCroiss = false;

        //        int a = 0;
        //        int b = 0;
        //        int c = 0;

        //        while (i < matImage.GetLength(0) && j < matImage.GetLength(1))
        //        {
        //            int[] tabIndicesCouleurs = { a, b, c };
        //            matImage[i, j] = new Pixel(tabIndicesCouleurs);
        //            matImage[j, i] = new Pixel(tabIndicesCouleurs);

        //            if (aCroiss)
        //            {
        //                a += 1;
        //                if (a > 254) { aCroiss = false; a = 255; bCroiss = true; }
        //            }
        //            /*else
        //            {
        //                a-=1; ;
        //                if (a < 1) { aCroiss = true; a = 0; }
        //            }*/
        //            if (bCroiss)
        //            {
        //                b += 1;
        //                if (b > 254) { bCroiss = false; b = 255; cCroiss = true; }
        //            }
        //            /*else
        //            {
        //                b-=1;
        //                if (b < 1) { bCroiss = true; b = 0; }
        //            }*/
        //            if (cCroiss)
        //            {
        //                c += 1;
        //                if (c > 254) { cCroiss = false; c = 255; }
        //            }
        //            /*else
        //            {
        //                c-=1;
        //                if (c < 1) { cCroiss = true; c = 0; }
        //            }*/

        //            i++;
        //            j++;
        //        }

        //        if (colonne > 0) { colonne--; }
        //        j = colonne;
        //        i = 0;
        //    }

        //}
        /// <summary>
        /// fonction qui transforme une matrice d'image en couleur en matrice d'image en noir et blanc
        /// </summary>
        public void NoirEtBlanc()
        {
            Pixel[,] matImageNoirEtBlanc = new Pixel[hauteurImage, largeurImage];
            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    int niveauDeGris = (matImage[i, j].Pixels[0] + matImage[i, j].Pixels[1] + matImage[i, j].Pixels[2]) / 3; //on fait la moyenne de rouge vert et bleu
                    int[] pixelGris = new int[3];
                    pixelGris[0] = niveauDeGris;
                    pixelGris[1] = niveauDeGris;
                    pixelGris[2] = niveauDeGris;
                    Pixel pixelgrispix = new Pixel(pixelGris);
                    matImageNoirEtBlanc[i, j] = pixelgrispix;
                }
            }
            matImage = matImageNoirEtBlanc;
        }
        /// <summary>
        /// fonction qui inverse les couleurs d'une matrice d'image
        /// </summary>
        /// <returns>retourne la matrice de l'image modifiée</returns>
        public Pixel[,] Negatif()
        {
            Pixel[,] imageNegatif = new Pixel[hauteurImage, largeurImage];
            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    int pixelRouge = 255 - matImage[i, j].Pixels[0];
                    int pixelVert = 255 - matImage[i, j].Pixels[1];
                    int pixelBleu = 255 - matImage[i, j].Pixels[2];
                    int[] newPixel = { pixelRouge, pixelVert, pixelBleu };
                    imageNegatif[i, j] = new Pixel(newPixel);
                }
            }
            return imageNegatif;
        }
        /// <summary>
        /// fonction qui permet d'appliquer des filtres de convolutions à des images 
        /// </summary>
        /// <param name="kernel"></param>
        public void Filtrage(double[,] kernel)
        {
            Pixel[,] matImageFinale = new Pixel[hauteurImage, largeurImage];
            int[] tabNull = { 0, 0, 0 };
            for (int i = 0; i < matImageFinale.GetLength(0); i++)
            {
                for (int j = 0; j < matImageFinale.GetLength(1); j++)
                {
                    matImageFinale[i, j] = new Pixel(tabNull);
                }
            }

            int[,] mat9Pixels = new int[kernel.GetLength(0), kernel.GetLength(1)];

            //AfficherImageOriginal(matImage);
            //AfficherMatriceDouble(kernel);
            for (int indexLigneMatImage = 0; indexLigneMatImage < matImage.GetLength(0) - (kernel.GetLength(0) - 1); indexLigneMatImage++)
            {
                for (int indexColonneMatImage = 0; indexColonneMatImage < matImage.GetLength(1) - (kernel.GetLength(1) - 1); indexColonneMatImage++)
                {
                    double somme; //somme de tous les octets de la mat9Pixels
                    double sommeKernel;
                    double sommeKernelPositif; //somme de tous les éléments du kernel
                    double sommeKernelNegatif;

                    for (int indexColonnePixel = 0; indexColonnePixel < 3; indexColonnePixel++)
                    {
                        somme = 0;
                        sommeKernel = 0;
                        sommeKernelPositif = 0;
                        sommeKernelNegatif = 0;

                        for (int indexLigneMat9Pixels = 0; indexLigneMat9Pixels < kernel.GetLength(0); indexLigneMat9Pixels++)
                        {
                            for (int compteur = 0; compteur < kernel.GetLength(1); compteur++)
                            {
                                mat9Pixels[indexLigneMat9Pixels, compteur] = matImage[indexLigneMatImage + indexLigneMat9Pixels, indexColonneMatImage + compteur].Pixels[indexColonnePixel];
                                somme += mat9Pixels[indexLigneMat9Pixels, compteur] * kernel[indexLigneMat9Pixels, compteur];
                                //sommeKernel += (kernel[indexLigneMat9Pixels, compteur]);

                                if (kernel[indexLigneMat9Pixels, compteur] > 0)
                                {
                                    sommeKernelPositif += kernel[indexLigneMat9Pixels, compteur];
                                }
                                else
                                {
                                    sommeKernelNegatif += kernel[indexLigneMat9Pixels, compteur];
                                }
                            }
                        }


                        if (sommeKernelPositif > Math.Abs(sommeKernelNegatif)) { sommeKernel = sommeKernelPositif; }
                        else { sommeKernel = Math.Abs(sommeKernelNegatif); }
                        if (sommeKernel == 0) { sommeKernel = 1; }
                        int nouvelOctet = Convert.ToInt32(Math.Abs(somme / sommeKernel));

                        matImageFinale[indexLigneMatImage + 1, indexColonneMatImage + 1].Pixels[indexColonnePixel] = nouvelOctet;

                        //AfficherMatriceInt(mat9Pixels);
                        //AfficherMatricePixels(matImageFinale);
                    }
                }
            }

            for (int indexLigne = 0; indexLigne < matImageFinale.GetLength(0); indexLigne += matImageFinale.GetLength(0) - 1)
            {
                for (int indexColonne = 0; indexColonne < matImageFinale.GetLength(1); indexColonne++)
                {
                    if (indexLigne == 0)
                    {
                        matImageFinale[0, indexColonne].Pixels[0] = matImageFinale[1, indexColonne].Pixels[0];
                        matImageFinale[0, indexColonne].Pixels[1] = matImageFinale[1, indexColonne].Pixels[1];
                        matImageFinale[0, indexColonne].Pixels[2] = matImageFinale[1, indexColonne].Pixels[2];
                    }
                    else if (indexLigne == matImageFinale.GetLength(0) - 1)
                    {
                        matImageFinale[matImageFinale.GetLength(0) - 1, indexColonne].Pixels[0] = matImageFinale[matImageFinale.GetLength(0) - 2, indexColonne].Pixels[0];
                        matImageFinale[matImageFinale.GetLength(0) - 1, indexColonne].Pixels[1] = matImageFinale[matImageFinale.GetLength(0) - 2, indexColonne].Pixels[1];
                        matImageFinale[matImageFinale.GetLength(0) - 1, indexColonne].Pixels[2] = matImageFinale[matImageFinale.GetLength(0) - 2, indexColonne].Pixels[2];
                    }
                }
            }

            for (int indexColonne = 0; indexColonne < matImageFinale.GetLength(1); indexColonne += matImageFinale.GetLength(1) - 1)
            {
                for (int indexLigne = 0; indexLigne < matImageFinale.GetLength(0); indexLigne++)
                {
                    if (indexColonne == 0)
                    {
                        matImageFinale[indexLigne, 0].Pixels[0] = matImageFinale[indexLigne, 1].Pixels[0];
                        matImageFinale[indexLigne, 0].Pixels[1] = matImageFinale[indexLigne, 1].Pixels[1];
                        matImageFinale[indexLigne, 0].Pixels[2] = matImageFinale[indexLigne, 1].Pixels[2];
                    }
                    else if (indexColonne == matImageFinale.GetLength(1) - 1)
                    {
                        matImageFinale[indexLigne, matImageFinale.GetLength(1) - 1].Pixels[2] = matImageFinale[indexLigne, matImageFinale.GetLength(1) - 2].Pixels[2];
                        matImageFinale[indexLigne, matImageFinale.GetLength(1) - 1].Pixels[1] = matImageFinale[indexLigne, matImageFinale.GetLength(1) - 2].Pixels[1];
                        matImageFinale[indexLigne, matImageFinale.GetLength(1) - 1].Pixels[0] = matImageFinale[indexLigne, matImageFinale.GetLength(1) - 2].Pixels[0];
                    }
                }

            }
            //AfficherMatricePixels(matImageFinale);
            matImage = matImageFinale;

        }
        /// <summary>
        /// fonction qui permet de doubler ou de diviser par deux les dimensions de l'image 
        /// </summary>
        /// <param name="choixRedimensionner"></param>
        public void Redimensionner(char choixRedimensionner)
        {
            Pixel[,] matNouvelleImage = null;
            if (choixRedimensionner == 'a')
            {
                matNouvelleImage = new Pixel[hauteurImage * 2, largeurImage * 2];
                for (int i = 0; i < matImage.GetLength(0); i++)
                {
                    for (int j = 0; j < matImage.GetLength(1); j++)
                    {
                        matNouvelleImage[1 + i * 2, j * 2] = matNouvelleImage[i * 2, 1 + j * 2] = matNouvelleImage[1 + i * 2, 1 + j * 2] = matNouvelleImage[i * 2, j * 2] = matImage[i, j];
                    }
                }
            }

            if (choixRedimensionner == 'r')
            {
                matNouvelleImage = new Pixel[Convert.ToInt32(hauteurImage / 2), Convert.ToInt32(largeurImage / 2)];

                for (int i = 0; i < matNouvelleImage.GetLength(0); i++)
                {
                    for (int j = 0; j < matNouvelleImage.GetLength(1); j++)
                    {
                        matNouvelleImage[i, j] = matImage[i * 2, j * 2];
                        //AfficherMatricePixels(matNouvelleImage);
                    }
                }
            }

            matImage = matNouvelleImage;
        }
        /// <summary>
        /// fonction qui permet transformer une image selon un axe de symétrie vertical 
        /// </summary>
        public void Miroir()
        {
            Pixel[,] matImageMiroir = new Pixel[hauteurImage, largeurImage];
            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    matImageMiroir[i, j] = matImage[i, matImage.GetLength(1) - j - 1];
                }
            }
            matImage = matImageMiroir;
        }
        /// <summary>
        /// fonction qui permet d'effectuer une rotation d'angle quelconque sur une image
        /// </summary>
        /// <param name="degre"></param>
        public void Flip(double degre)
        {
            double a = 0;
            double b = 0;
            if (((degre >= 0) && (degre < 90)) || ((degre > 270) && (degre <= 360)))
            {
                a = Largeur * Math.Cos(degre * (Math.PI / 180)) + Hauteur * Math.Sin(degre * (Math.PI / 180));
                b = Largeur * Math.Sin(degre * (Math.PI / 180)) + Hauteur * Math.Cos(degre * (Math.PI / 180));
            }
            else if ((degre == 90) || (degre == 270))
            {
                a = Largeur;
                b = Hauteur;
            }
            else if (degre == 180)
            {
                a = Hauteur;
                b = Largeur;
            }
            else
            {
                a = Hauteur * Math.Cos((degre - 90) * (Math.PI / 180)) + Largeur * Math.Sin((degre - 90) * (Math.PI / 180));
                b = Hauteur * Math.Sin((degre - 90) * (Math.PI / 180)) + Largeur * Math.Cos((degre - 90) * (Math.PI / 180));
            }
            Pixel[,] newMatric = new Pixel[Convert.ToInt32(a), Convert.ToInt32(b)];
            double x = 0;
            double y = 0;
            double x0 = (newMatric.GetLength(0) / 2) + (matImage.GetLength(0) - newMatric.GetLength(0)) / 2; //newMatric.GetLength(0)/2;
            double y0 = (newMatric.GetLength(1) / 2) + (matImage.GetLength(1) - newMatric.GetLength(1)) / 2; //newMatric.GetLength(1)/2;
            for (int i = 0; i < newMatric.GetLength(0); i++)
            {
                for (int j = 0; j < newMatric.GetLength(1); j++)
                {
                    x = (Math.Cos(Math.PI * (degre / 180)) * (i - x0)) + (Math.Sin(Math.PI * (degre / 180)) * (j - y0)) + x0;
                    y = -(Math.Sin(Math.PI * (degre / 180)) * (i - x0)) + (Math.Cos(Math.PI * (degre / 180)) * (j - y0)) + y0;
                    if ((x >= 0) && (y >= 0) && (x < matImage.GetLength(0)) && (y < matImage.GetLength(1)))
                    {
                        newMatric[i, j] = matImage[Convert.ToInt32(Math.Truncate(x)), Convert.ToInt32(Math.Truncate(y))];
                    }
                }
            }
            matImage = newMatric;
        }
        /// <summary>
        /// fonction qui permet de créer une fractale (l'ensemble de Mandelbrot)
        /// </summary>
        public void Mandelbrot()
        {
            int[] tab0 = new int[3];
            Pixel couleur = new Pixel(tab0);
            int iteration = 0;
            int[] tab = { 0, 0, 0 };
            Pixel x = new Pixel(tab);
            Complexes z = new Complexes(0, 0);
            for (int i = 0; i < Hauteur; i++)
            {
                for (int j = 0; j < Largeur; j++)
                {
                    double a = (double)(i - (Largeur / 2)) / (double)(Largeur / 4);
                    double b = (double)(j - (Hauteur / 2)) / (double)(Hauteur / 4);
                    Complexes c = new Complexes(a, b);
                    z.a = 0;
                    z.b = 0;
                    iteration = 0;
                    do
                    {
                        iteration++;
                        z.Square();


                        z.Add(c);
                        if (z.Module() > 2.0) break;
                    } while (iteration < 1000);

                    if (iteration == 1000)
                    {
                        matImage[i, j] = x;
                    }
                    else
                    {
                        //tab0[1] = iteration * 255 / 1000;
                        tab0 = EscapeTimeCouleur(iteration);

                        Pixel y = new Pixel(tab0);

                        matImage[i, j] = y;
                    }
                }
            }
        }
        /// <summary>
        /// fonction qui permet de colorer les fractales 
        /// </summary>
        /// <param name="n"></param>
        /// <returns>retourne un tableau d'entiers qui sera utilisé pour créer un pixel et colorer une fractale</returns>
        public int[] EscapeTimeCouleur(int n)
        {
            double b = (1 / (20 * Math.Sqrt(0.1))) * (1 / Math.Log(2));
            int couleur = Convert.ToInt32(255 - (255 * ((1 + Math.Cos(2 * b * Math.Log(n + 1))) / 2)));
            int[] tab = { couleur, 0, 0 };
            return tab;

        }
        /// <summary>
        /// fonction qui permet de créer une fractale de julia
        /// </summary>
        /// <param name="u">partie réelle</param>
        /// <param name="n">partie imaginaire</param>
        public void Julia(double u, double n)
        {
            int[] tab0 = new int[3];
            Pixel couleur = new Pixel(tab0);
            int iteration = 0;
            int[] tab = { 0, 0, 0 };
            Pixel x = new Pixel(tab);
            Complexes c = new Complexes(u, n);
            for (int i = 0; i < Largeur; i++)
            {
                for (int j = 0; j < Hauteur; j++)
                {
                    double a = (double)(i - (Largeur / 2)) / (double)(Largeur / 4);
                    double b = (double)(j - (Hauteur / 2)) / (double)(Hauteur / 4);
                    Complexes z = new Complexes(a, b);
                    iteration = 0;
                    do
                    {
                        iteration++;
                        z.Square();
                        //z.Square();
                        //z.Square();
                        z.Add(c);
                        if (z.Module() > 2.0) break;
                    } while (iteration < 50);

                    if (iteration == 50)
                    {
                        matImage[i, j] = x;
                    }
                    else
                    {
                        //tab0[1] = iteration * 255 / 1000;
                        tab0 = EscapeTimeCouleur(iteration);

                        Pixel y = new Pixel(tab0);

                        matImage[i, j] = y;
                    }
                }
            }
        }
        public void MatHistogrammeLuminosite()
        {
            int[] tabHistogramme = new int[256];

            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    for (int o = 0; o < 3; o++)
                    {
                        tabHistogramme[matImage[i, j].Pixels[o]]++;
                        //pour chaque octet on incrémente l'indice qui lui est attribué dans le tableau tabHistogramme (on compte la répartition des octets)
                    }
                }
            }

            int indiceMax = 0;
            for (int i = 0; i < tabHistogramme.Length; i++)
            {
                if (tabHistogramme[i] > indiceMax) indiceMax = tabHistogramme[i];
                //l'histogramme prendra comme hauteur la plus grande des valeurs du tableau
            }
            int coeffDecalage = 1;
            if (indiceMax / 256 >= 1) { coeffDecalage = Convert.ToInt32((indiceMax / 256) * 1.5); }

            Pixel[,] matHistogramme = new Pixel[indiceMax, 256 * coeffDecalage]; //nous multiplions la taille en largeur de l'histogramme par la valeur de coeffDecalage pour une meilleur visibilité
            int indiceColonneMatHistogramme = 0;
            //nous devons séparer l'indice de colonne de la matrice et celui de la colonne du tableau car ces deux indices sont différents
            //en raison du décalage que nous appliquons
            int[] tabPixelBlanc = { 255, 255, 255 };
            Pixel pixelBlanc = new Pixel(tabPixelBlanc);
            int[] tabPixelNoir = { 0, 0, 0 };
            Pixel pixelNoir = new Pixel(tabPixelNoir);

            for (int indiceTab = 0; indiceTab < 256; indiceTab++)
            {
                for (int indiceLigneMatHistogramme = 0; indiceLigneMatHistogramme < matHistogramme.GetLength(0); indiceLigneMatHistogramme++)
                {
                    if (indiceLigneMatHistogramme < tabHistogramme[indiceTab])
                    {
                        for (int numPixel = 0; numPixel < coeffDecalage; numPixel++)
                        {
                            matHistogramme[indiceLigneMatHistogramme, indiceColonneMatHistogramme + numPixel] = pixelBlanc;
                            //si l'indice j est inférieur à la hauteur de la colonne de l'octet numéro i, onplace un pixel blanc dans l'histogramme

                        }
                    }
                    else
                    {
                        for (int numPixel = 0; numPixel < coeffDecalage; numPixel++)
                        {
                            matHistogramme[indiceLigneMatHistogramme, indiceColonneMatHistogramme + numPixel] = pixelNoir;
                        }
                    }
                }
                indiceColonneMatHistogramme += coeffDecalage;
            }
            matImage = matHistogramme;
        }
        /// <summary>
        /// fonction qui crée un histogramme des couleurs présentes dans une image
        /// </summary>
        public void MatHistogrammeCouleurs()
        {
            int[] tabHistogrammeRouge = new int[256];
            int[] tabHistogrammeVert = new int[256];
            int[] tabHistogrammeBleu = new int[256];

            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    //on passe en revue toutes les cases de la matrice, on prends chaque tableau associé à une couleur et on incrémente l'index du tableau égal
                    //à l'indice de couleur observé

                    tabHistogrammeRouge[matImage[i, j].Pixels[0]]++;
                    tabHistogrammeVert[matImage[i, j].Pixels[1]]++;
                    tabHistogrammeBleu[matImage[i, j].Pixels[2]]++;
                }
            }

            int indiceMax = 0;
            for (int i = 0; i < 256; i++)
            {
                if (tabHistogrammeRouge[i] > indiceMax) indiceMax = tabHistogrammeRouge[i];
                if (tabHistogrammeVert[i] > indiceMax) indiceMax = tabHistogrammeVert[i];
                if (tabHistogrammeBleu[i] > indiceMax) indiceMax = tabHistogrammeBleu[i];
                //l'histogramme prendra comme hauteur la plus grande des valeurs des tableaux
            }
            int coeffDecalage = 1;
            if (indiceMax / 256 >= 1) { coeffDecalage = Convert.ToInt32((indiceMax / 256) * 1.5); }

            Pixel[,] matHistogramme = new Pixel[indiceMax, 256 * coeffDecalage]; //nous multiplions la taille en largeur de l'histogramme par la valeur de coeffDecalage pour une meilleur visibilité
            int indiceColonneMatHistogramme = 0;
            //nous devons séparer l'indice de colonne de la matrice et celui de la colonne du tableau car ces deux indices sont différents
            //en raison du décalage que nous appliquons

            int[] pixelBlanc = { 255, 255, 255 };
            int[] pixelNoir = { 0, 0, 0 };

            for (int i = 0; i < matHistogramme.GetLength(0); i++)
            {
                for (int j = 0; j < matHistogramme.GetLength(1); j++)
                {
                    matHistogramme[i, j] = new Pixel(pixelNoir);
                    //on place le fond de l'histogramme de sortie
                }
            }

            for (int indiceTab = 0; indiceTab < 256; indiceTab++)
            {
                for (int indiceLigneMatHistogramme = 0; indiceLigneMatHistogramme < matHistogramme.GetLength(0); indiceLigneMatHistogramme++)
                {
                    //on parcourt toutes les cases de la matrice,
                    //on prend une colonne, on parcourt sa hauteur et dès qu'on dépasse l'indice associé à cette colonne dans le tableau
                    //on change la couleur placée dans la matrice
                    if (indiceLigneMatHistogramme < tabHistogrammeRouge[indiceTab])
                    {


                        for (int numPixel = 0; numPixel < coeffDecalage; numPixel++)
                        {
                            matHistogramme[indiceLigneMatHistogramme, indiceColonneMatHistogramme + numPixel].Pixels[0] = 255;
                            //si l'indice indiceLigneMatHistogramme est inférieur à la hauteur de la colonne de l'octet numéro i, on change l'octet "rouge" dans l'histogramme

                        }
                    }
                    if (indiceLigneMatHistogramme < tabHistogrammeVert[indiceTab])
                    {
                        for (int numPixel = 0; numPixel < coeffDecalage; numPixel++)
                        {
                            matHistogramme[indiceLigneMatHistogramme, indiceColonneMatHistogramme + numPixel].Pixels[1] = 255;
                            //si l'indice indiceLigneMatHistogramme est inférieur à la hauteur de la colonne de l'octet numéro i, on change l'octet "vert" dans l'histogramme

                        }
                    }
                    if (indiceLigneMatHistogramme < tabHistogrammeBleu[indiceTab])
                    {
                        for (int numPixel = 0; numPixel < coeffDecalage; numPixel++)
                        {
                            matHistogramme[indiceLigneMatHistogramme, indiceColonneMatHistogramme + numPixel].Pixels[2] = 255;
                            //si l'indice indiceLigneMatHistogramme est inférieur à la hauteur de la colonne de l'octet numéro i, on change l'octet "bleu" dans l'histogramme

                        }
                    }
                }
                indiceColonneMatHistogramme += coeffDecalage;



            }
            matImage = matHistogramme;
        }
        /// <summary>
        /// fonction qui permet de cacher une image dans une autre image
        /// </summary>
        /// <param name="uneImage"></param>
        public void Cacher_Une_Image_Dans_Une_Image(MonImage uneImage)
        {
            Pixel[,] nouvelleMatrice = new Pixel[matImage.GetLength(0), matImage.GetLength(1)];
            string nouveauBinaire = "";
            int temporaire = 0;
            int temporaire2 = 0;
            string mot = "";
            string mot2 = "";
            int[] tab = { 255, 255, 255 };
            int[] tab0 = new int[3];
            Pixel unPixel = new Pixel(tab);
            for (int i = 0; i < matImage.GetLength(0); i++)
            {
                for (int j = 0; j < matImage.GetLength(1); j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        temporaire = matImage[i, j].Pixels[k];
                        temporaire2 = uneImage.matImage[i, j].Pixels[k];
                        if (nouvelleMatrice[i, j] == null)
                        {
                            nouvelleMatrice[i, j] = unPixel;
                        }
                        //on convertit la valeur de chaque octet en binaire
                        mot = Convert.ToString(temporaire, 2);
                        mot2 = Convert.ToString(temporaire2, 2);
                        //on vérifie que la longueur de la chaîne de caractères est de 8
                        while (mot.Length < 8)
                        {
                            mot = "0" + mot;
                        }
                        while (mot2.Length < 8)
                        {
                            mot2 = "0" + mot2;
                        }
                        //on crée une nouvelle chaîne de caractères constituée des quatre premiers caractères de mot et mot2 correspondant
                        //aux 4 premiers chiffres de la valeur des octets en binaire
                        nouveauBinaire = mot.Substring(0, 4) + mot2.Substring(0, 4);
                        tab0[k] = Convert.ToInt32(nouveauBinaire, 2);
                    }
                    //on crée un nouveau pixel correspondant à un pixel de l'image finale
                    Pixel aPixel = new Pixel(tab0);
                    nouvelleMatrice[i, j] = aPixel;
                }
            }
            matImage = nouvelleMatrice;
        }
        //public void FusionImages(Pixel[,] matImage1, Pixel[,] matImage2)
        //{
        //    //if (matImage1.GetLength(0) == matImage2.GetLength(0) && matImage1.GetLength(1) == matImage2.GetLength(1))
        //    {
        //        for (int i = 0; i < matImage1.GetLength(0); i++)
        //        {
        //            for (int j = 0; j < matImage1.GetLength(1); j++)
        //            {
        //                for (int o = 0; o < 3; o++)
        //                {
        //                    matImage1[i, j].Pixels[o] = (matImage1[i, j].Pixels[o] + matImage2[i, j].Pixels[o]) / 2;
        //                }
        //            }
        //        }
        //    }
        //    matImage = matImage1;
        //}
        //public void Neon()
        //{
        //    int[] tabPixelNoir = { 0, 0, 0 };
        //    double coeff = 4;
        //    for (int i = 0; i < matImage.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < matImage.GetLength(1); j++)
        //        {
        //            if (matImage[i, j].Pixels != tabPixelNoir)
        //            {
        //                for (int o = 0; o < 3; o++)
        //                {
        //                    if (matImage[i, j].Pixels[o] * coeff < 256)
        //                    {
        //                        matImage[i, j].Pixels[o] = Convert.ToInt32(matImage[i, j].Pixels[o] * coeff);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// fonction qui permet de retrouver les deux images à partir d'une image cachée dans une autre
        /// </summary>
        /// <param name="image2"></param>
        public void Retrouver_Images(MonImage image2)
        {
            Pixel[,] nouvelleMatrice = new Pixel[matImage.GetLength(0), matImage.GetLength(1)];
            int[] tab = { 255, 255, 255 };
            int[] tab0 = new int[3];
            int[] tab1 = new int[3];
            Pixel unPixel = new Pixel(tab);
            string chaine = "";
            string chaine1 = "";
            string chaine2 = "";
            for (int i = 0; i < nouvelleMatrice.GetLength(0); i++)
            {
                for (int j = 0; j < nouvelleMatrice.GetLength(1); j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (nouvelleMatrice[i, j] == null)
                        {
                            nouvelleMatrice[i, j] = unPixel;
                        }
                        if (image2.matImage[i, j] == null)
                        {
                            image2.matImage[i, j] = unPixel;
                        }
                        //on convertit la valeur de chaque octet en binaire
                        chaine = Convert.ToString(matImage[i, j].Pixels[k], 2);
                        //on vérifie que la longueur de la chaine est de 8
                        while (chaine.Length < 8)
                        {
                            chaine = "0" + chaine;
                        }
                        //on sépare la chaine en deux chaines de longueur 4
                        chaine1 = chaine.Substring(0, 4);
                        chaine2 = chaine.Substring(4, 4);
                        while (chaine1.Length < 8)
                        {
                            chaine1 += "0";
                        }
                        while (chaine2.Length < 8)
                        {
                            chaine2 += "0";
                        }
                        //on convertit les chaines (chaine1 et chaine2) en int puis on les met dans un tableau de 3 valeurs
                        //on le fait 2 fois pour retrouver les 2 images
                        tab0[k] = Convert.ToInt32(chaine1, 2);
                        tab1[k] = Convert.ToInt32(chaine2, 2);
                    }
                    //on crée un nouveau pixel à partir du tableau d'entiers puis on donne la valeur de ce pixel à la matrice
                    Pixel aPixel = new Pixel(tab0);
                    Pixel bPixel = new Pixel(tab1);
                    nouvelleMatrice[i, j] = aPixel;
                    image2.matImage[i, j] = bPixel;
                }
            }
            matImage = nouvelleMatrice;
        }
    }
}






