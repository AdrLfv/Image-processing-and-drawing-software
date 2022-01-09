using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Media;

namespace TEST_lire_ecrire_image
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Le programme est en cours d'exécution");

            string fichier = "./Images/coco.bmp";

            string path = "./Images/Sortie.bmp";
            //MonImage monImage = new MonImage(10000, 10000);

            //monImage.Mandelbrot();

            ConsoleKeyInfo cki;
            do
            {
                Console.Clear();
                Console.WriteLine("\nMenu :\n\n"
                                 + "Sélectionnez le numeroEntreéro pour la commande désirée \n"
                                 + "0.Mandelbrot \n"
                                 + "1.Julia \n"
                                 + "2.NoirEtBlanc \n"
                                 + "3.Redimention\n"
                                 + "4.Filtrage \n"
                                 + "5.Rotation \n"
                                 + "6.Miroir \n"
                                 + "7.Histogramme couleurs \n"
                                 + "8.Histogramme luminosité \n"
                                 + "9.Cacher une image dans une image \n"
                                 + "10.Retrouver des images cachées \n"
                                 + "11.Générer un QR Code \n"
                                 + "12.Décoder un QR Code");

                int numeroEntre = Convert.ToInt32(Console.ReadLine());
                Console.Clear();
                if (numeroEntre == 10)
                {
                    fichier = "./Images/Sortie.bmp";
                }
                MonImage monImage = new MonImage(fichier);
                switch (numeroEntre)
                {
                    case 0:
                        monImage.Mandelbrot();
                        break;
                    case 1:
                        Console.WriteLine("Saisir la partie réelle > ");
                        double a = Convert.ToDouble(Console.ReadLine());
                        Console.WriteLine("Saisir la partie imaginaire > ");
                        double b = Convert.ToDouble(Console.ReadLine());
                        monImage.Julia(a, b);
                        Console.WriteLine();

                        break;

                    case 2:
                        monImage.NoirEtBlanc();
                        break;

                    case 3:
                        char choixRedimentionner = 'b';
                        while (choixRedimentionner != 'a' && choixRedimentionner != 'r')
                        {
                            Console.Clear();
                            Console.WriteLine("Entrer a pour un agrandissement ou r pour un rétrécissement");
                            bool charValide = char.TryParse(Console.ReadLine(), out choixRedimentionner);
                            if (!charValide) choixRedimentionner = 'b';
                        }
                        Console.Clear();
                        monImage.Redimensionner(choixRedimentionner);
                        break;

                    case 4:
                        //double[,] kernel = new double[,] { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
                        //double[,] kernel = new double[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
                        double[,] kernel = new double[,] { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
                        //double[,] kernel = new double[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
                        //double[,] kernel = new double[,] { { 0, 1, 0 }, { 1, 4, 1 }, { 0, 1, 0 } };
                        //double[,] kernel = new double[,] { { 1, 0, -1 }, { 0, 0, 0 }, { -1, 0, 1 } };
                        //double[,] kernel = new double[,] { { 0.111111, 0.111111, 0.111111 }, { 0.111111, 0.111111, 0.111111 }, { 0.111111, 0.111111, 0.111111 } };
                        //double[,] kernel = new double[,] { { 1/16, 2 / 16, 1 / 16 }, { 2 / 16, 4 / 16, 2 / 16 }, { 1 / 16, 2 / 16, 1 / 16 } };
                        //double[,] kernel = new double[,] { { 1, 4, 6, 4, 1 }, { 4, 16, 24, 16, 4 }, { 6, 24, 36, 24, 6 }, { 4, 16, 24, 16, 4 }, { 1, 4, 6, 4, 1 } }; for (int i = 0; i < kernel.GetLength(0); i++) { for (int j = 0; j < kernel.GetLength(1); j++) { kernel[i, j] *= 1 / 256; } }
                        monImage.Filtrage(kernel);
                        break;

                    case 5:
                        Console.WriteLine("Saisir degre > ");
                        double degre = Convert.ToDouble(Console.ReadLine());
                        monImage.Flip(degre);
                        break;

                    case 6:
                        monImage.Miroir();
                        break;

                    case 7:
                        monImage.MatHistogrammeCouleurs();
                        break;

                    case 8:
                        monImage.MatHistogrammeLuminosite();
                        break;

                    case 9:
                        string fichier2 = "./Images/lac.bmp";
                        MonImage imageCachee = new MonImage(fichier2);
                        monImage.Cacher_Une_Image_Dans_Une_Image(imageCachee);
                        //path = "./Images/ImageDansUneImgage";
                        break;

                    case 10:
                        MonImage image2 = new MonImage(monImage.Hauteur, monImage.Largeur);
                        monImage.Retrouver_Images(image2);
                        image2.From_Image_To_File("./Images/Image0.bmp");
                        path = "./Images/Image1.bmp";
                        break;

                    case 11:
                        Console.WriteLine("Saisir votre chaine de caractères > ");
                        string texte = Convert.ToString(Console.ReadLine());
                        texte = texte.ToUpper();
                        QrCode unQRCode = new QrCode(texte);
                        unQRCode.Generer_QrCode();
                        break;

                    case 12:
                        MonImage qrCode = new MonImage("./Images/Qrcode.bmp");
                        qrCode.Redimensionner('r');
                        QrCode unCode = new QrCode(qrCode);
                        unCode.Decoder_QrCode();
                        break;
                }
                monImage.From_Image_To_File(path);
                //monImage.From_Image_To_File2();
                Console.WriteLine("\nAppuyez sur Echap pour quitter ou entrez une commande");
                cki = Console.ReadKey();
            } while (cki.Key != ConsoleKey.Escape);
        }
    }
}



