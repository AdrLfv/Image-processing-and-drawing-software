using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST_lire_ecrire_image
{
    public class QrCode
    {
        private int version;
        private string texte;
        private MonImage qrCode;
        private Pixel pixelNoir;
        private Pixel pixelBlanc;
        private Pixel pixelBleu;  //pour vérifier que ça fonctionne à enlever à terme

        public QrCode(string texte)
        {
            this.texte = texte;
            qrCode = new MonImage(21, 21);
            int[] tab0 = { 0, 0, 0 };
            int[] tab1 = { 255, 255, 255 };
            int[] tab2 = { 255, 0, 0 };
            int[] tab3 = { 0, 0, 255 };
            pixelNoir = new Pixel(tab0);
            pixelBlanc = new Pixel(tab1);
            pixelBleu = new Pixel(tab2);
        }
        public QrCode(MonImage qrCode)
        {
            this.qrCode = qrCode;
        }
        public int Version
        {
            get { return this.version; }
            set { this.version = value; }
        }
        public string Texte
        {
            get { return this.texte; }
        }
        public MonImage UnQrCode
        {
            get { return this.qrCode; }
            set { this.qrCode = value; }
        }
        public Pixel PixelBlanc
        {
            get { return this.pixelBlanc; }
        }
        public Pixel PixelBleu
        {
            get { return this.pixelBleu; }
        }
        public Pixel PixelNoir
        {
            get { return this.pixelNoir; }
        }
        /// <summary>
        /// Génère un QR Code à partir d'un texte
        /// </summary>
        public void Generer_QrCode()
        {
            string chaineOctets = "0010";
            if (texte.Length < 26) version = 1;
            else if (texte.Length < 48) version = 2;
            chaineOctets = Longueur_Texte(chaineOctets);
            List<string> motDecoupe = new List<string>();
            if (texte.Length % 2 != 0) //on répartie deux lettres par deux lettres dans la liste
            {
                for (int i = 0; i < texte.Length - 2; i += 2)
                {
                    motDecoupe.Add(Convert.ToString(texte[i]) + texte[i + 1]);
                }
                motDecoupe.Add(Convert.ToString(texte[texte.Length - 1]));
            }
            else
            {
                for (int i = 0; i < texte.Length - 1; i += 2) //s'il ne reste plus qu'une lettre à répartir
                {
                    motDecoupe.Add(Convert.ToString(texte[i]) + texte[i + 1]);
                }
            }
            List<string> motDecoupeBinary = new List<string>();
            int motDecoupeTemporaire = 0;
            for (int i = 0; i < motDecoupe.Count; i++)
            {
                motDecoupeTemporaire = 0;
                for (int j = 0; j < motDecoupe[i].Length; j++)
                //conversion des lettres dans la liste motDecoupeBinary
                {
                    motDecoupeTemporaire += (int)(Math.Pow(45, j) * ValeurLettre(motDecoupe[i][motDecoupe[i].Length - 1 - j]));
                }

                //Console.WriteLine("Valeur de deux lettres :");
                //Console.WriteLine(motDecoupeTemporaire);

                motDecoupeBinary.Add(Convert.ToString(motDecoupeTemporaire, 2));
                if (motDecoupe[i].Length != 1)
                //si il ne reste plus qu'un caractère à placer, nous n'aurons besoin que de 6 bits et non 11 pour le coder en binaire
                {
                    while (motDecoupeBinary[i].Length < 11) motDecoupeBinary[i] = "0" + motDecoupeBinary[i];
                }
                else while (motDecoupeBinary[i].Length < 6) motDecoupeBinary[i] = "0" + motDecoupeBinary[i];
            }


            foreach (string element in motDecoupeBinary)
            {
                Console.WriteLine("Element1 :");
                Console.WriteLine(element);
            }


            for (int i = 0; i < motDecoupeBinary.Count; i++)
            //écriture de la chaine binaire obtenue à partir du tableau
            {
                chaineOctets += motDecoupeBinary[i];
            }
            int compteur = 0;
            while ((chaineOctets.Length < 152) && (compteur < 4))
            //remplissage de 0 pour atteindre les 152 bits
            {
                chaineOctets += "0";
                compteur++;
            }
            for (int i = 0; i < chaineOctets.Length % 8; i++)
            //on complete la chaine d'octets avec des 0 pour atteindre un multiple de 8  	 

            {
                chaineOctets += "0";
            }
            //indication de la fin du code binaire
            for (int i = chaineOctets.Length; i < /*(((152 - chaineOctets.Length) / 8) - 7)*/152; i += 16)
            {
                if (/*(((152 - chaineOctets.Length) / 8) - 7)*/152 - i == 8) chaineOctets += "11101100";
                else
                {
                    chaineOctets += "1110110000010001";
                }
            }

            Console.WriteLine();
            byte[] tab = new byte[19];
            string temporaire = "";
            //on recopie chaque groupe de 8 octets dans un string, on convertie ce string en binaire et on le place dans le tableau de byte
            for (int i = 0; i < tab.Length; i++)
            {
                temporaire = "";
                for (int j = 0; j < 8; j++)
                {
                    temporaire += Convert.ToString(chaineOctets[i * 8 + j]);
                }
                tab[i] = Convert.ToByte(temporaire, 2);
            }
            //détermine le nombre de bits de données pour coder les données et le code d'erreur
            byte[] errorCode = ReedSolomonAlgorithm.Encode(tab, 7, ErrorCorrectionCodeType.QRCode);

            //conversion du code d'erreur en binaire et placement dans la chaine d'octets
            for (int i = 0; i < errorCode.Length; i++)
            {
                temporaire = Convert.ToString(errorCode[i], 2);
                while (temporaire.Length < 8) temporaire = "0" + temporaire;
                chaineOctets += temporaire;
            }
            for (int i = 0; i < chaineOctets.Length; i++)
            {
                Console.Write(chaineOctets[i]);
            }
            //placement des motifs de recherche (trois carrés dans les coins)
            Placement_Motif_Recherche(version);
            Placement_Pointilles(version);
            qrCode.MatImage[7, 8] = pixelNoir;
            Placement_Donnees(chaineOctets);
            Appliquer_Masque();
            qrCode.Redimensionner('a');
            qrCode.From_Image_To_File("./Images/Qrcode.bmp");
        }
        /// <summary>
        /// conversion de la longueur du texte en binaire pour son placement dans le QrCode
        /// </summary>
        /// <param name="chaineOctets"></param>
        /// <returns>la longueur du texte en binaire</returns>
        public string Longueur_Texte(string chaineOctets)
        {
            string longueurMotBinary = "";
            longueurMotBinary = Convert.ToString(texte.Length, 2);
            //si la taille de la chaine binaire contenant la longueur est inférieure à 9 on la complète avec des 0
            while (longueurMotBinary.Length < 9)
            {
                longueurMotBinary = "0" + longueurMotBinary;
            }
            chaineOctets += longueurMotBinary;
            return chaineOctets;
        }
        /// <summary>
        /// placement des motifs de recherche (trois carrés dans les coins)
        /// </summary>
        /// <param name="version"></param>
        public void Placement_Motif_Recherche(int version)
        {
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 21; j++)
                {
                    qrCode.MatImage[i, j] = PixelBleu;
                }
            }
            //on place les pixels du paterne de detection en haut à gauche
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((((j == 5) || (j == 1)) && ((i != 0) && (i != 6))) || (((i == 5) || (i == 1)) && ((j != 0) && (j != 6))) || (i == 7) || (j == 7))
                    {
                        qrCode.MatImage[i, j] = pixelBlanc;
                    }
                    else qrCode.MatImage[i, j] = pixelNoir;
                }
            }
            //on place les pixels du paterne de detection en bas à droite
            for (int i = qrCode.MatImage.GetLength(0) - 8; i < qrCode.MatImage.GetLength(0); i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 8; j < qrCode.MatImage.GetLength(1); j++)
                {
                    if ((((j == qrCode.MatImage.GetLength(1) - 6) || (j == qrCode.MatImage.GetLength(1) - 2)) && ((i != qrCode.MatImage.GetLength(0) - 1) && (i != qrCode.MatImage.GetLength(0) - 7))) || (((i == qrCode.MatImage.GetLength(0) - 6) || (i == qrCode.MatImage.GetLength(0) - 2)) && ((j != qrCode.MatImage.GetLength(1) - 1) && (j != qrCode.MatImage.GetLength(1) - 7))) || (i == qrCode.MatImage.GetLength(0) - 8) || (j == qrCode.MatImage.GetLength(1) - 8))
                    {
                        qrCode.MatImage[i, j] = pixelBlanc;
                    }
                    else qrCode.MatImage[i, j] = pixelNoir;
                }
            }
            //on place les pixels du paterne de detection en bas à gauche
            for (int i = qrCode.MatImage.GetLength(0) - 8; i < qrCode.MatImage.GetLength(0); i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((((j == 5) || (j == 1)) && ((i != qrCode.MatImage.GetLength(0) - 1) && (i != qrCode.MatImage.GetLength(0) - 7))) || (((i == qrCode.MatImage.GetLength(0) - 6) || (i == qrCode.MatImage.GetLength(0) - 2)) && ((j != 0) && (j != 6))) || (i == qrCode.MatImage.GetLength(0) - 8) || (j == 7))
                    {
                        qrCode.MatImage[i, j] = pixelBlanc;
                    }
                    else qrCode.MatImage[i, j] = pixelNoir;
                }
            }
        }
        /// <summary>
        /// placement des timing patterns (ligne et colonne en pointillé)
        /// </summary>
        /// <param name="version"></param>
        public void Placement_Pointilles(int version)
        {
            //placement des 2 lignes en pointillés, les "timming patterns" verticaux
            for (int i = 8; i < qrCode.MatImage.GetLength(0) - 8; i++)
            {
                if (i % 2 == 0)
                {
                    qrCode.MatImage[i, 6] = pixelNoir;
                }
                else qrCode.MatImage[i, 6] = pixelBlanc;
            }
            //on place les "timming patterns" horizontaux
            for (int j = 8; j < qrCode.MatImage.GetLength(1) - 8; j++)
            {
                if (j % 2 == 0)
                {
                    qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j] = pixelNoir;
                }
                else qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j] = pixelBlanc;
            }
        }
        /// <summary>
        /// placement des données sur le QR Code en repectant l'ordre
        /// </summary>
        /// <param name="chaineOctets"></param>
        public void Placement_Donnees(string chaineOctets)
        {
            int compteur = 0;
            for (int i = 0; i < qrCode.MatImage.GetLength(0) - 9; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 1; j >= qrCode.MatImage.GetLength(1) - 2; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }

            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 0; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 3; j >= qrCode.MatImage.GetLength(1) - 4; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = 0; i < qrCode.MatImage.GetLength(0) - 9; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 5; j >= qrCode.MatImage.GetLength(1) - 6; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 0; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 7; j >= qrCode.MatImage.GetLength(1) - 8; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = 0; i <= qrCode.MatImage.GetLength(0) - 1; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 9; j >= qrCode.MatImage.GetLength(1) - 10; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 11; j >= qrCode.MatImage.GetLength(1) - 12; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = 8; i <= qrCode.MatImage.GetLength(0) - 10; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 13; j >= qrCode.MatImage.GetLength(1) - 14; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 8; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 16; j >= qrCode.MatImage.GetLength(1) - 17; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = 8; i <= qrCode.MatImage.GetLength(0) - 10; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 18; j >= qrCode.MatImage.GetLength(1) - 19; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 8; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 20; j >= qrCode.MatImage.GetLength(1) - 21; j--)
                {
                    if (qrCode.MatImage[i, j] == pixelBleu)
                    {
                        if (chaineOctets[compteur] == '1')
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelBlanc;
                            else qrCode.MatImage[i, j] = pixelNoir;
                        }
                        else
                        {
                            if ((i + j) % 2 == 0) qrCode.MatImage[i, j] = pixelNoir;
                            else qrCode.MatImage[i, j] = pixelBlanc;
                        }
                        compteur++;
                    }
                }
            }
        }
        /// <summary>
        /// remplissage des cases prévues pour l'information sur le masque
        /// </summary>
        public void Appliquer_Masque()
        {
            string mask = "111011111000100";
            int compteur = 0;
            //écriture du masque horizontal du paterne haut-gauche
            for (int j = 0; j < 9; j++)
            {
                if (qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j] == pixelBleu)
                {
                    if (mask[compteur] == '0') qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j] = pixelBlanc;
                    else qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j] = pixelNoir;
                    compteur++;
                }
            }
            //écriture du masque vertical du paterne haut-gauche
            for (int i = qrCode.MatImage.GetLength(0) - 8; i < qrCode.MatImage.GetLength(0); i++)
            {
                if (qrCode.MatImage[i, 8] == pixelBleu)
                {
                    if (mask[compteur] == '0') qrCode.MatImage[i, 8] = pixelBlanc;
                    else qrCode.MatImage[i, 8] = pixelNoir;
                    compteur++;
                }
            }
            //écriture du masque vertical du paterne bas-gauche
            compteur = 0;
            for (int i = 0; i < 7; i++)
            {
                if (qrCode.MatImage[i, 8] == pixelBleu)
                {
                    if (mask[compteur] == '0') qrCode.MatImage[i, 8] = pixelBlanc;
                    else qrCode.MatImage[i, 8] = pixelNoir;
                    compteur++;
                }
            }
            //écriture du masque horizontal du paterne haut-droit
            for (int j = qrCode.MatImage.GetLength(1) - 8; j < qrCode.MatImage.GetLength(1); j++)
            {
                if (qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j] == pixelBleu)
                {
                    if (mask[compteur] == '0') qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j] = pixelBlanc;
                    else qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j] = pixelNoir;
                    compteur++;
                }
            }
        }
        /// <summary>
        /// fonction qui associe à un nombre un caractère
        /// </summary>
        /// <param name="charactere"></param>
        /// <returns>le caractère associé au nombre</returns>
        public char LettreValeur(int nombre)
        {
            if (nombre == 10) return 'A';
            else if (nombre == 11) return 'B';
            else if (nombre == 12) return 'C';
            else if (nombre == 13) return 'D';
            else if (nombre == 14) return 'E';
            else if (nombre == 15) return 'F';
            else if (nombre == 16) return 'G';
            else if (nombre == 17) return 'H';
            else if (nombre == 18) return 'I';
            else if (nombre == 19) return 'J';
            else if (nombre == 20) return 'K';
            else if (nombre == 21) return 'L';
            else if (nombre == 22) return 'M';
            else if (nombre == 23) return 'N';
            else if (nombre == 24) return 'O';
            else if (nombre == 25) return 'P';
            else if (nombre == 26) return 'Q';
            else if (nombre == 27) return 'R';
            else if (nombre == 28) return 'S';
            else if (nombre == 29) return 'T';
            else if (nombre == 30) return 'U';
            else if (nombre == 31) return 'V';
            else if (nombre == 32) return 'W';
            else if (nombre == 33) return 'X';
            else if (nombre == 34) return 'Y';
            else if (nombre == 35) return 'Z';
            else if (nombre == 36) return ' ';
            else if (nombre == 37) return '$';
            else if (nombre == 38) return '%';
            else if (nombre == 39) return '*';
            else if (nombre == 40) return '+';
            else if (nombre == 41) return '-';
            else if (nombre == 42) return '.';
            else if (nombre == 43) return '/';
            else if (nombre == 44) return ':';
            else return '0';
        }
        /// <summary>
        /// fonction qui associe un nombre à un caractère
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns>le nombre associé au caractère</returns>
        public int ValeurLettre(char charactere)
        {
            if (charactere == 'A') return 10;
            else if (charactere == 'B') return 11;
            else if (charactere == 'C') return 12;
            else if (charactere == 'D') return 13;
            else if (charactere == 'E') return 14;
            else if (charactere == 'F') return 15;
            else if (charactere == 'G') return 16;
            else if (charactere == 'H') return 17;
            else if (charactere == 'I') return 18;
            else if (charactere == 'J') return 19;
            else if (charactere == 'K') return 20;
            else if (charactere == 'L') return 21;
            else if (charactere == 'M') return 22;
            else if (charactere == 'N') return 23;
            else if (charactere == 'O') return 24;
            else if (charactere == 'P') return 25;
            else if (charactere == 'Q') return 26;
            else if (charactere == 'R') return 27;
            else if (charactere == 'S') return 28;
            else if (charactere == 'T') return 29;
            else if (charactere == 'U') return 30;
            else if (charactere == 'V') return 31;
            else if (charactere == 'W') return 32;
            else if (charactere == 'X') return 33;
            else if (charactere == 'Y') return 34;
            else if (charactere == 'Z') return 35;
            else if (charactere == ' ') return 36;
            else if (charactere == '$') return 37;
            else if (charactere == '%') return 38;
            else if (charactere == '*') return 39;
            else if (charactere == '+') return 40;
            else if (charactere == '-') return 41;
            else if (charactere == '.') return 42;
            else if (charactere == '/') return 43;
            else if (charactere == ':') return 44;
            else return 0;
        }
        public void Afficher_QrCode()
        {
            int[] tabPixBlanc = { 255, 255, 255 };
            Pixel pixelBlanc = new Pixel(tabPixBlanc);
            int[] tabPixNoir = { 0, 0, 0 };
            Pixel pixelNoirc = new Pixel(tabPixNoir);
            for (int i = 0; i < qrCode.MatImage.GetLength(0); i++) //on parcourt toutes les lignes de l'matrice
            {
                for (int j = 0; j < qrCode.MatImage.GetLength(1); j++) //on parcourt toutes les colonnes de l'matrice
                {
                    if (qrCode.MatImage[i, j].Pixels[0] == qrCode.MatImage[i, j].Pixels[1] && qrCode.MatImage[i, j].Pixels[1] == qrCode.MatImage[i, j].Pixels[2] && qrCode.MatImage[i, j].Pixels[2] == 255)
                    {
                        Console.Write("8" + " ");
                    }

                    else if (qrCode.MatImage[i, j].Pixels[0] == qrCode.MatImage[i, j].Pixels[1] && qrCode.MatImage[i, j].Pixels[1] == qrCode.MatImage[i, j].Pixels[2] && qrCode.MatImage[i, j].Pixels[2] == 0)
                    {
                        Console.Write("-" + " ");
                    }

                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        public void Decoder_QrCode()
        {
            Console.WriteLine("Est un QR Code :");
            Console.WriteLine(Verification_QrCode());
            Console.WriteLine("Masque :");
            Console.WriteLine(ObtentionDuMasque());


            string motFinal = Recuperation_Donnees();
            Console.WriteLine("QR Code décodé :");
            Console.WriteLine(motFinal);

        }
        /// <summary>
        /// vérfie que tous les patterns sont bien présents
        /// </summary>
        /// <param name="version"></param>
        /// <returns>vrai si tous les patterns sont présents, non sinon</returns>
        public bool Verification_QrCode()
        {
            bool estUnQrCode = true;
            //on vérifie les pixels du paterne de detection en haut à gauche
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((((j == 5) || (j == 1)) && ((i != 0) && (i != 6))) || (((i == 5) || (i == 1)) && ((j != 0) && (j != 6))) || (i == 7) || (j == 7))
                    //si on se trouve sur le carré de pixels blancs
                    {
                        if (qrCode.MatImage[i, j].Pixels[0] != 255 && qrCode.MatImage[i, j].Pixels[1] != 255 && qrCode.MatImage[i, j].Pixels[2] != 255) { estUnQrCode = false; }
                        //si les bits du pixel observé ne sont pas ceux d'un pixel blanc
                    }
                    else if (qrCode.MatImage[i, j].Pixels[0] != 0 && qrCode.MatImage[i, j].Pixels[1] != 0 && qrCode.MatImage[i, j].Pixels[2] != 0) { estUnQrCode = false; }
                    //si les bits du pixel observé ne sont pas ceux d'un pixel noir
                }
            }
            //on vérifie les pixels du paterne de detection en bas à droite
            for (int i = qrCode.MatImage.GetLength(0) - 8; i < qrCode.MatImage.GetLength(0); i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 8; j < qrCode.MatImage.GetLength(1); j++)
                {
                    if ((((j == qrCode.MatImage.GetLength(1) - 6) || (j == qrCode.MatImage.GetLength(1) - 2)) && ((i != qrCode.MatImage.GetLength(0) - 1) && (i != qrCode.MatImage.GetLength(0) - 7))) || (((i == qrCode.MatImage.GetLength(0) - 6) || (i == qrCode.MatImage.GetLength(0) - 2)) && ((j != qrCode.MatImage.GetLength(1) - 1) && (j != qrCode.MatImage.GetLength(1) - 7))) || (i == qrCode.MatImage.GetLength(0) - 8) || (j == qrCode.MatImage.GetLength(1) - 8))
                    {
                        if (qrCode.MatImage[i, j].Pixels[0] != 255 && qrCode.MatImage[i, j].Pixels[1] != 255 && qrCode.MatImage[i, j].Pixels[2] != 255) { estUnQrCode = false; }
                        //si les bits du pixel observé ne sont pas ceux d'un pixel blanc
                    }
                    else if (qrCode.MatImage[i, j].Pixels[0] != 0 && qrCode.MatImage[i, j].Pixels[1] != 0 && qrCode.MatImage[i, j].Pixels[2] != 0) { estUnQrCode = false; }
                    //si les bits du pixel observé ne sont pas ceux d'un pixel noir
                }
            }
            //on vérifie les pixels du paterne de detection en bas à gauche
            for (int i = qrCode.MatImage.GetLength(0) - 8; i < qrCode.MatImage.GetLength(0); i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((((j == 5) || (j == 1)) && ((i != qrCode.MatImage.GetLength(0) - 1) && (i != qrCode.MatImage.GetLength(0) - 7))) || (((i == qrCode.MatImage.GetLength(0) - 6) || (i == qrCode.MatImage.GetLength(0) - 2)) && ((j != 0) && (j != 6))) || (i == qrCode.MatImage.GetLength(0) - 8) || (j == 7))
                    {
                        if (qrCode.MatImage[i, j].Pixels[0] != 255 && qrCode.MatImage[i, j].Pixels[1] != 255 && qrCode.MatImage[i, j].Pixels[2] != 255) { estUnQrCode = false; }
                        //si les bits du pixel observé ne sont pas ceux d'un pixel blanc
                    }
                    else if (qrCode.MatImage[i, j].Pixels[0] != 0 && qrCode.MatImage[i, j].Pixels[1] != 0 && qrCode.MatImage[i, j].Pixels[2] != 0) { estUnQrCode = false; }
                    //si les bits du pixel observé ne sont pas ceux d'un pixel noir
                }
            }

            //verification des 2 lignes en pointillés, les "timming patterns" verticaux
            for (int i = 8; i < qrCode.MatImage.GetLength(0) - 8; i++)
            {
                if (i % 2 == 0)
                {
                    if (qrCode.MatImage[i, 6].Pixels[0] != 0 && qrCode.MatImage[i, 6].Pixels[1] != 0 && qrCode.MatImage[i, 6].Pixels[2] != 0) { estUnQrCode = false; }
                }
                else if (qrCode.MatImage[i, 6].Pixels[0] != 255 && qrCode.MatImage[i, 6].Pixels[1] != 255 && qrCode.MatImage[i, 6].Pixels[2] != 255) { estUnQrCode = false; }
            }
            //on vérifie les "timming patterns" horizontaux
            for (int j = 8; j < qrCode.MatImage.GetLength(1) - 8; j++)
            {
                if (j % 2 == 0)
                {
                    if (qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j].Pixels[0] != 0 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j].Pixels[1] != 0 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j].Pixels[2] != 0) { estUnQrCode = false; }
                }
                else if (qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j].Pixels[0] != 255 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j].Pixels[1] != 255 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 7, j].Pixels[2] != 255) { estUnQrCode = false; }
            }

            return estUnQrCode;

        }
        public string ObtentionDuMasque()
        {
            string mask = "";
            //obtention du masque horizontal du paterne haut-gauche
            for (int j = 0; j < 9; j++)
            {
                if (qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j].Pixels[0] != 0 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j].Pixels[1] != 0 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j].Pixels[2] != 0) { mask += '0'; }
                else mask += '1'; ;
            }

            //obtention du masque horizontal du paterne haut-gauche
            for (int i = qrCode.MatImage.GetLength(0) - 8; i < qrCode.MatImage.GetLength(0); i++)
            {
                if (qrCode.MatImage[i, 8].Pixels[0] != 0 && qrCode.MatImage[i, 8].Pixels[1] != 0 && qrCode.MatImage[i, 8].Pixels[2] != 0) { mask += '0'; }
                else mask += '1';
            }

            //obtention du masque horizontal du paterne bas-gauche
            for (int i = 0; i < 7; i++)
            {

                if (qrCode.MatImage[i, 8].Pixels[0] != 0 && qrCode.MatImage[i, 8].Pixels[1] != 0 && qrCode.MatImage[i, 8].Pixels[2] != 0) { mask += '0'; }
                else mask += '1';

            }
            //obtention du masque horizontal du paterne haut-droit
            for (int j = qrCode.MatImage.GetLength(1) - 8; j < qrCode.MatImage.GetLength(1); j++)
            {
                if (qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j].Pixels[0] != 0 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j].Pixels[1] != 0 && qrCode.MatImage[qrCode.MatImage.GetLength(0) - 9, j].Pixels[2] != 0) { mask += '0'; }
                else mask += '1';
            }
            return mask;
        }
        /// <summary>
        /// récupère les données trouvées dans le QR Code et les transforme en caractères
        /// </summary>
        /// <returns>la chaine de caractères codée dans le QR Code</returns>
        public string Recuperation_Donnees()
        {
            string chaineOctets = "";
            for (int i = 0; i < qrCode.MatImage.GetLength(0) - 9; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 1; j >= qrCode.MatImage.GetLength(1) - 2; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 0; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 3; j >= qrCode.MatImage.GetLength(1) - 4; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = 0; i < qrCode.MatImage.GetLength(0) - 9; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 5; j >= qrCode.MatImage.GetLength(1) - 6; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 0; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 7; j >= qrCode.MatImage.GetLength(1) - 8; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = 0; i <= qrCode.MatImage.GetLength(0) - 8; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 9; j >= qrCode.MatImage.GetLength(1) - 10; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 6; i <= qrCode.MatImage.GetLength(0) - 1; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 9; j >= qrCode.MatImage.GetLength(1) - 10; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 1; i >= qrCode.MatImage.GetLength(0) - 6; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 11; j >= qrCode.MatImage.GetLength(1) - 12; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 8; i >= 0; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 11; j >= qrCode.MatImage.GetLength(1) - 12; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = 8; i <= qrCode.MatImage.GetLength(0) - 10; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 13; j >= qrCode.MatImage.GetLength(1) - 14; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 8; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 16; j >= qrCode.MatImage.GetLength(1) - 17; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = 8; i <= qrCode.MatImage.GetLength(0) - 10; i++)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 18; j >= qrCode.MatImage.GetLength(1) - 19; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            for (int i = qrCode.MatImage.GetLength(0) - 10; i >= 8; i--)
            {
                for (int j = qrCode.MatImage.GetLength(1) - 20; j >= qrCode.MatImage.GetLength(1) - 21; j--)
                {
                    if ((qrCode.MatImage[i, j].Pixels[0] == 255) && (qrCode.MatImage[i, j].Pixels[1] == 255) && (qrCode.MatImage[i, j].Pixels[2] == 255))
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '1';
                        else chaineOctets += '0';
                    }
                    else
                    {
                        if ((i + j) % 2 == 0) chaineOctets += '0';
                        else chaineOctets += '1';
                    }
                }
            }
            string chaineDonnees = "";

            string longueurTexteBinary = "";
            for (int i = 4; i < 13; i++)
            {
                longueurTexteBinary += chaineOctets[i];
            }
            int longueurTexte = Convert.ToInt32(longueurTexteBinary, 2);
            if (longueurTexte%2==0)
            {
                for (int i = 13; i < (longueurTexte / 2) * 11 + 13; i++)
                {
                    chaineDonnees += chaineOctets[i];
                }
            }
            else
            {
                for (int i = 13; i < ((longueurTexte / 2) * 11 + 19); i++)
                {
                    chaineDonnees += chaineOctets[i];
                }
            }

            Console.WriteLine("chaineDonnees: ");
            Console.WriteLine(chaineDonnees);
            Console.WriteLine(chaineDonnees.Length);

            List<int> listeValeursLettres = new List<int>();
            string lettresBinairesTemporaires = "";

            for (int i = 0, j = 0; i + j < chaineDonnees.Length; i += 11)
            {
                for (j = 0; j < 11; j++)
                {
                    lettresBinairesTemporaires += chaineDonnees[i + j];
                }

                Console.WriteLine("lettresBinairesTemporaires " + (i + 11) / 11 + " :");
                Console.WriteLine(lettresBinairesTemporaires);
                listeValeursLettres.Add(Convert.ToInt32(lettresBinairesTemporaires, 2));
                lettresBinairesTemporaires = "";
            }
            if (chaineDonnees.Length % 11 != 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    lettresBinairesTemporaires += chaineDonnees[chaineDonnees.Length - 6 + i];
                }
                Console.WriteLine("dernieres lettresBinairesTemporaires :");
                Console.WriteLine(lettresBinairesTemporaires);
                listeValeursLettres.Add(Convert.ToInt32(lettresBinairesTemporaires, 2));
            }
            else
            {
                for (int i = 0; i < 11; i++)
                {
                    lettresBinairesTemporaires += chaineDonnees[chaineDonnees.Length - 11 + i];
                }
                Console.WriteLine("dernieres lettresBinairesTemporaires :");
                Console.WriteLine(lettresBinairesTemporaires);
                listeValeursLettres.Add(Convert.ToInt32(lettresBinairesTemporaires, 2));
            }

            foreach (int element in listeValeursLettres)
            {
                Console.WriteLine("Element1 :");
                Console.WriteLine(element);
            }

            string motFinal = "";
            int quotient = 0;
            for (int i = 0; i < listeValeursLettres.Count; i++)
            {
                quotient = Math.DivRem(listeValeursLettres[i], 45, out int reste);
                if (quotient != 0)
                {
                    motFinal += LettreValeur(quotient);
                    motFinal += LettreValeur(reste);
                }
                else
                {
                    motFinal += LettreValeur(reste);
                }
                Console.WriteLine(motFinal);
            }
            return motFinal;
        }
    }
}

