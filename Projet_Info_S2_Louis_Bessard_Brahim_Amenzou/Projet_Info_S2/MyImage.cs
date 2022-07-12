using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using ReedSolomon;

namespace Projet_Info_S2
{
    class MyImage
    {
        //Déclaration
        string name;
        int size;
        int hauteur;
        int largeur;
        int bits;
        Pixel[,] image;

        //Propriétés
        public Pixel[,] Image
        {
            get { return image; }
        }

        //Constructeur
        /// <summary>
        /// Constructeur de la class MyImage
        /// Sépare le fichier en plusieurs parties, largeur, hauteur, taille, et bien sur l'image qui est mise dans une matrice de Pixel
        /// </summary>
        /// <param name="filename"></param>
        public MyImage(string filename)
        {
            name = filename; //Nom du fichier

            byte[] myfile = File.ReadAllBytes(name); //Création d'un tableau de bits à partir du fichier

            //Initialisation des caractéristiques de l'image à partir des métadonées
            largeur = Convert.ToInt32(Convert.ToInt32(myfile[18]) + Convert.ToInt32(myfile[19]) * 256 + Convert.ToInt32(myfile[20]) * 65536 + Convert.ToInt32(myfile[21]) * 4294967296);
            Console.WriteLine("La largeur est " + largeur);
            hauteur = Convert.ToInt32(Convert.ToInt32(myfile[22]) + Convert.ToInt32(myfile[23]) * 256 + Convert.ToInt32(myfile[24]) * 65536 + Convert.ToInt32(myfile[25]) * 4294967296);
            Console.WriteLine("La hauteur est " + hauteur);
            size = largeur * hauteur; //Taille du fichier en bits
            Console.WriteLine("La taille du fichier est " + size);
            bits = myfile[28] + myfile[29] * 256;
            Console.WriteLine("bits = " + bits);

            image = new Pixel[hauteur, largeur]; //Créatrion de l'emplacement image, de taille [hauteur, largeur]

            //Remplissage de la matrice de pixels
            int count = 0; //Initialisation d'un compteur permettant de l'indice [i,j] du pixel à l'indice du bit du fichier
                           //54 (métadonnées) + 0/1/2 (la couleur du pixel) + compteur (permet de bien sauter les 3 bits et pas un seul)

            for(int i = 0; i < hauteur; i++) //Remplissage de la matrice
            {
                for(int j = 0; j < largeur; j++)
                {
                    image[i, j] = new Pixel();
                    image[i, j].Bleu = myfile[54 + count];
                    image[i, j].Vert = myfile[54 + 1 + count];
                    image[i, j].Rouge = myfile[54 + 2 + count];
                    count += 3;
                }
            }
        }

        //Opérations
        /// <summary>
        /// Transforme une matrice de Pixel en fichier bitmap, l'enregistre puis le lit
        /// </summary>
        public void From_Image_To_File() //Création d'un fichier (byte[]) à partir d'une image (tableau de pixels)
        {
            byte[] nouvelleimage  = new byte[54 + image.GetLength(0) * image.GetLength(1) * 3]; //Création de la nouvelle image

            //Récupération des nouvelles informations de l'image, pour les insérer dans les métadonnées
            byte[] tabsize = Convertir_Int_To_Endian0(size);
            byte[] tabheight = Convertir_Int_To_Endian0(hauteur);
            byte[] tablength = Convertir_Int_To_Endian0(largeur);

            //Récupération des métadonnées d'une image .bmp
            byte[] myfile = File.ReadAllBytes(name); //Récupération du fichier entier
            for (int i = 0; i < 54; i++) nouvelleimage[i] = myfile[i]; //Recopie des métadonnées

            //On remplace les métadonnées de taille, hauteur et largeur
            for (int i = 2; i < 6; i++) nouvelleimage[i] = tabsize[i - 2]; //Taille
            for (int i = 18; i < 22; i++) nouvelleimage[i] = tablength[i - 18]; //Largeur
            for (int i = 22; i < 26; i++) nouvelleimage[i] = tabheight[i - 22]; //Hauteur

            //Remplissage du reste des données à partir de notre image actuelle
            int count = 0;

            for (int i = 0; i < hauteur; i++) //Remplissage de la matrice
            {
                for (int j = 0; j < largeur; j++)
                {
                    nouvelleimage[54 + count] = image[i, j].Bleu;
                    nouvelleimage[54 + 1 + count] = image[i, j].Vert;
                    nouvelleimage[54 + 2 + count] = image[i, j].Rouge;
                    count += 3;
                }
            }

            //Enregistrement et lecture de l'image
            File.WriteAllBytes("NouvelleImage.bmp", nouvelleimage); //Enregistrement de l'image dans un fichier appelé NouvelleImage.bmp
            Process p = new();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = "NouvelleImage.bmp";
            p.Start(); //Ouverture de l'image
        }
        /// <summary>
        /// Convertit un tableau de byte en un entier
        /// </summary>
        /// <param name="tab"></param>
        /// <returns>un entier égal au nombre en octet en entrée</returns>
        public int Convertir_Endian_To_Int(byte[] tab) //Convertisseur octets -> entier
        {
            int a = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                int b = tab[i];
                for (int j = 0; j < i; j++)
                {
                    b = b * 256;
                }
                a = a + b;
            }
            return a;
        }
        /// <summary>
        /// Prend une valeur ainsi qu'une taille afin de transformer la valeur (int) en un tableau de byte de la taille sélectionnée
        /// </summary>
        /// <param name="val"></param>
        /// <param name="taille"></param>
        /// <returns>tableau de byte de la taille taille et de la valeur val</returns>
        public byte[] Convertir_Int_To_Endian(int val, int taille) //Convertisseur entier -> octets
        {
            byte[] tab = new byte[taille];
            for (int i = taille - 1; i >= 0; i--)
            {
                int puissance = 1;
                for (int j = 0; j < i; j++)
                {
                    puissance = puissance * 256;
                }
                tab[i] = Convert.ToByte(val / (puissance));
                val = val % (puissance);
            }
            return tab;
        }
        /// <summary>
        /// Convertit une valeur en un talbeau de byte (sans la taille en entrée)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public byte[] Convertir_Int_To_Endian0(int val) //Convertisseur entier -> octets
        {
            return BitConverter.GetBytes(val);
        }

        /// <summary>
        /// Convertit une valeur en un bit unique (la valeur doit être inférieure à 256)
        /// </summary>
        /// <param name="val"></param>
        /// <returns>un byte simple égal à la valeur en entrée</returns>
        public byte Convertir_Int_To_Endian2(int val) //Convertisseur entier -> octets
        {
            byte[] bite = BitConverter.GetBytes(val);
            byte bite2 = bite[0];
            return bite2;
        }
        /// <summary>
        /// Retourne une image (comme si on la regardait dans un miroir)
        /// </summary>
        public void ImageMirroir()
        {
            Pixel[,] NewImage = new Pixel[image.GetLength(0), image.GetLength(1)]; //Création d'une seconde matrice

            //Copie de l'image dans la matrice avec pour instruction : nouvelle ligne = length - ligne
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    NewImage[i, image.GetLength(1) - 1 - j] = image[i, j];
                }
            }
            image = NewImage;
        }
        /// <summary>
        /// Aggrandis une image de la valeur donnée en entrée (nombre de pixels final = 3² * valeur * nombre de pixels initial)
        /// </summary>
        /// <param name="valeur"></param>
        public void AggrandirImage(int valeur)
        {
            //Création d'une seconde matrice de côté 3 fois plus grand et de compteurs
            Pixel[,] NewImage = new Pixel[image.GetLength(0) * valeur, image.GetLength(1) * valeur];
            /*for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int k = 0; k < valeur; k++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        for (int l = 0; l < valeur; l++)
                        {
                            NewImage[valeur * i + k, valeur * j + l] = image[i, j];
                        }
                    }
                }
            }*/
            for (int i = 0; i < NewImage.GetLength(0); i++)
            {
                for (int j = 0; j < NewImage.GetLength(1); j++)
                {
                    NewImage[i, j] = image[i / valeur, j / valeur];
                }
            }
            hauteur = hauteur * valeur;
            largeur = largeur * valeur;
            size = size * valeur;
            image = NewImage;
        }
        /// <summary>
        /// Tourne l'image de l'angle sélectionné (ne marche pas aussi bien que prévu)
        /// </summary>
        /// <param name="angle"></param>
        public void TournerImage(int angle)
        {
            Pixel[,] NewImage = new Pixel[0,0];
            if (angle == 90 || angle == 270)
            {
                NewImage = new Pixel[image.GetLength(1), image.GetLength(0)];
                if (angle == 90)
                {
                    for (int i = 0; i < image.GetLength(0); i++)
                    {
                        for (int j = 0; j < image.GetLength(1); j++)
                        {
                            NewImage[NewImage.GetLength(0) - 1 - j, i] = image[i, j];
                        }
                    }
                    image = NewImage;
                    int newlargeur = hauteur;
                    hauteur = largeur;
                    largeur = newlargeur;
                }
                else
                {
                    for (int i = 0; i < image.GetLength(0); i++)
                    {
                        for (int j = 0; j < image.GetLength(1); j++)
                        {
                            NewImage[j, NewImage.GetLength(1) - 1 - i] = image[i, j];
                        }
                    }
                    image = NewImage;
                    int newlargeur = hauteur;
                    hauteur = largeur;
                    largeur = newlargeur;
                }
            }
            else if (angle == 180)
            {
                NewImage = new Pixel[image.GetLength(0), image.GetLength(1)];
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        NewImage[NewImage.GetLength(0) - 1 - i, NewImage.GetLength(1) - 1 - j] = image[i, j];
                    }
                }
                image = NewImage;
            }
            
            else Console.WriteLine("L'angle doit être 90°, 180° ou 270°");

            LargeurX4();

            /*LargeurX4();

            double radian = (angle * Math.PI) / 180;
            int cosinus = Convert.ToInt32(Math.Cos(radian));
            int sinus = Convert.ToInt32(Math.Sin(radian));
            byte[,,] tab3d = new byte[hauteur, largeur, 3];

            for (int i = 0; i < largeur; i++)
            {
                for (int j = 0; j < hauteur; j++)
                {
                    tab3d[j, i, 0] = image[j, i].Bleu;
                    tab3d[j, i, 1] = image[j, i].Vert;
                    tab3d[j, i, 2] = image[j, i].Rouge;
                }
            }

            int x1 = -hauteur * sinus;
            int y1 = hauteur * cosinus;
            int x2 = largeur * cosinus - hauteur * sinus;
            int y2 = hauteur * cosinus + largeur * cosinus;
            int x3 = largeur * cosinus;
            int y3 = largeur * sinus;

            int AbsMin = Math.Min(0, Math.Min(x1, Math.Min(x2, x3)));
            int OrdMin = Math.Min(0, Math.Min(y1, Math.Min(y2, y3)));
            int AbsMax = Math.Max(0, Math.Max(x1, Math.Max(x2, x3)));
            int OrdMax = Math.Max(0, Math.Max(y1, Math.Max(y2, y3)));
            int NouvelleLargeur = Convert.ToInt32(AbsMax - AbsMin);
            int NouvelleHauteur = Convert.ToInt32(OrdMax - OrdMin);

            Pixel[,] nouvelleImage = new Pixel[NouvelleHauteur, NouvelleLargeur];

            for (int i = 0; i < NouvelleLargeur; i++)
            {
                for (int j = 0; j < NouvelleHauteur; j++)
                {
                    nouvelleImage[j, i] = new Pixel();
                    int OrigineX = (i + AbsMin) * cosinus + (j + OrdMin) * sinus;
                    int OrigineY = (j + OrdMin) * cosinus - (i + AbsMin) * sinus;
                    if (OrigineX >= 0 && OrigineY < hauteur && OrigineY >= 0 && OrigineX < largeur)
                    {
                        nouvelleImage[j, i].Bleu = tab3d[OrigineY, OrigineX, 0];
                        nouvelleImage[j, i].Vert = tab3d[OrigineY, OrigineX, 1];
                        nouvelleImage[j, i].Rouge = tab3d[OrigineY, OrigineX, 2];
                    }
                }
            }
            largeur = NouvelleLargeur;
            hauteur = NouvelleHauteur;
            image = nouvelleImage;*/
        }
        /// <summary>
        /// Transforme une image couleur en une image en gris plus ou moins foncé
        /// </summary>
        public void ImageNuanceGris()
        {
            byte moyenne = 0;

            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    moyenne = Convert.ToByte((image[i, j].Bleu + image[i, j].Vert + image[i, j].Rouge) / 3);
                    image[i, j].Bleu = moyenne;
                    image[i, j].Vert = moyenne;
                    image[i, j].Rouge = moyenne;
                }
            }
        }
        /// <summary>
        /// Transforme une image couleur en image en noir et blanc uniquement
        /// </summary>
        public void ImageNoirBlanc() //Met une image en noir et blanc
        {
            byte moyenne = 0;

            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    moyenne = Convert.ToByte((image[i, j].Bleu + image[i, j].Vert + image[i, j].Rouge) / 3);

                    if(moyenne > 127)
                    {
                        image[i, j].Bleu = 255;
                        image[i, j].Vert = 255;
                        image[i, j].Rouge = 255;
                    }
                    else
                    {
                        image[i, j].Bleu = 0;
                        image[i, j].Vert = 0;
                        image[i, j].Rouge = 0;
                    }
                }
            }
        }
        /// <summary>
        /// Rétrécit l'image (divise par 9 le nombre de pixels)
        /// </summary>
        public void RetrecirImage()
        {
            //Création d'une seconde matrice de côté 3 fois plus petit
            Pixel[,] NewImage = new Pixel[image.GetLength(0) / 3, image.GetLength(1) / 3];
            byte moyenne = 0;

            for (int i = 0; i < NewImage.GetLength(0); i++)
            {
                for (int j = 0; j < NewImage.GetLength(1); j++)
                {
                    NewImage[i, j] = new Pixel();
                    moyenne = Convert.ToByte((image[3 * i, 3 * j].Bleu + image[3 * i + 1, 3 * j].Bleu + image[3 * i + 2, 3 * j].Bleu + image[3 * i, 3 * j + 1].Bleu + image[3 * i + 1, 3 * j + 1].Bleu + image[3 * i + 2, 3 * j + 1].Bleu + image[3 * i + 1, 3 * j + 2].Bleu + image[3 * i + 1, 3 * j + 2].Bleu + image[3 * i + 2, 3 * j + 2].Bleu) / 9);
                    NewImage[i, j].Bleu = moyenne;

                    moyenne = Convert.ToByte((image[3 * i, 3 * j].Vert + image[3 * i + 1, 3 * j].Vert + image[3 * i + 2, 3 * j].Vert + image[3 * i, 3 * j + 1].Vert + image[3 * i + 1, 3 * j + 1].Vert + image[3 * i + 2, 3 * j + 1].Vert + image[3 * i + 1, 3 * j + 2].Vert + image[3 * i + 1, 3 * j + 2].Vert + image[3 * i + 2, 3 * j + 2].Vert) / 9);
                    NewImage[i, j].Vert = moyenne;

                    moyenne = Convert.ToByte((image[3 * i, 3 * j].Rouge + image[3 * i + 1, 3 * j].Rouge + image[3 * i + 2, 3 * j].Rouge + image[3 * i, 3 * j + 1].Rouge + image[3 * i + 1, 3 * j + 1].Rouge + image[3 * i + 2, 3 * j + 1].Rouge + image[3 * i + 1, 3 * j + 2].Rouge + image[3 * i + 1, 3 * j + 2].Rouge + image[3 * i + 2, 3 * j + 2].Rouge) / 9);
                    NewImage[i, j].Rouge = moyenne;
                }
            }
            image = NewImage;
            hauteur = hauteur / 3;
            largeur = largeur / 3;
            size = size / 3;
            LargeurX4();
        }
        /// <summary>
        /// Réajuste la largeur de notre image car elle doit être impérativement un multiple de 4 (ajout d'une à 3 colonnes noires à l'image)
        /// </summary>
        public void LargeurX4()
        {
            //Réajuste la largeur de la matrice pour que ça soit un multiple de 4
            if (largeur % 4 != 0)
            {
                Pixel[,] NouvelleImage = new Pixel[hauteur, ((largeur / 4) + 1) * 4];
                for (int j = 0; j < hauteur; j++)
                {
                    for (int i = 0; i < NouvelleImage.GetLength(1); i++)
                    {
                        if (i < largeur) NouvelleImage[j, i] = image[j, i];
                        else
                        {
                            NouvelleImage[j, i] = new Pixel();
                            NouvelleImage[j, i].Bleu = 0x00;
                            NouvelleImage[j, i].Vert = 0x00;
                            NouvelleImage[j, i].Rouge = 0x00;
                        }
                    }
                }
                image = NouvelleImage;
                largeur = ((largeur / 4) +1) * 4;
            }
        }
        /// <summary>
        /// Floute l'image de notre class
        /// </summary>
        public void ImageFloue()
        {
            //Matrice de convolution de floutage
            double coef = 0.11111;
            int[,] convolution = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            Convolution(convolution, coef);
        }
        /// <summary>
        /// Met en évidence les bords de notre image
        /// </summary>
        public void ImageBordsRenforces()
        {
            Pixel[,] imageRenfo = new Pixel[image.GetLength(0), image.GetLength(1)];
            for (int i = 0; i < hauteur; i++)
                for (int j = 0; j < largeur; j++)
                    imageRenfo[i, j] = new Pixel();

            //On initialise des valeurs provisoires pour pouvoir les utiliser ensuite pour modifier nos pixels
            int valeurProvisoireBleu = 0;
            int valeurProvisoireVert = 0;
            int valeurProvisoireRouge = 0;

            for (int i = 0; i < imageRenfo.GetLength(1); i++) //On met les bords à 0
            {
                imageRenfo[0, i].Bleu = 0;
                imageRenfo[0, i].Vert = 0;
                imageRenfo[0, i].Rouge = 0;

                imageRenfo[imageRenfo.GetLength(0) - 1, i].Bleu = 0;
                imageRenfo[imageRenfo.GetLength(0) - 1, i].Vert = 0;
                imageRenfo[imageRenfo.GetLength(0) - 1, i].Rouge = 0;

            }
            for (int j = 0; j < imageRenfo.GetLength(0); j++) //On met les bords à 0
            {
                imageRenfo[j, 0].Bleu = 0;
                imageRenfo[j, 0].Vert = 0;
                imageRenfo[j, 0].Rouge = 0;

                imageRenfo[j, imageRenfo.GetLength(1) - 1].Bleu = 0;
                imageRenfo[j, imageRenfo.GetLength(1) - 1].Vert = 0;
                imageRenfo[j, imageRenfo.GetLength(1) - 1].Rouge = 0;
            }
            for (int i = 1; i < (hauteur) - 1; i++)
            {
                for (int j = 1; j < (largeur) - 1; j++)
                {
                    //On calcule nos nouvelles valeurs de chaque pixel
                    valeurProvisoireBleu = (-2 * image[i, j - 1].Bleu) + (2 * image[i, j].Bleu); //On multiplie par 2 pour mieux s'apercevoir du rendu
                    if (valeurProvisoireBleu < 0)
                        valeurProvisoireBleu = 0;
                    if (valeurProvisoireBleu > 255)
                        valeurProvisoireBleu = 255;

                    valeurProvisoireVert = (-2 * image[i, j - 1].Vert) + (2 * image[i, j].Vert);
                    if (valeurProvisoireVert < 0)
                        valeurProvisoireVert = 0;
                    if (valeurProvisoireVert > 255)
                        valeurProvisoireVert = 255;

                    valeurProvisoireRouge = (-2 * image[i, j - 1].Rouge) + (2 * image[i, j].Rouge);
                    if (valeurProvisoireRouge < 0)
                        valeurProvisoireRouge = 0;
                    if (valeurProvisoireRouge > 255)
                        valeurProvisoireRouge = 255;

                    //On change notre valeur par la nouvelle valeur
                    imageRenfo[i, j].Bleu = Convert.ToByte(valeurProvisoireBleu);
                    imageRenfo[i, j].Vert = Convert.ToByte(valeurProvisoireVert);
                    imageRenfo[i, j].Rouge = Convert.ToByte(valeurProvisoireRouge);
                }
            }
            image = imageRenfo;
        }
        /// <summary>
        /// Détecte les contours de l'image
        /// </summary>
        public void ImageDetectionContour()
        {
            //Matrice de convolution de détection de contour
            //int[,] convolution = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
            //Convolution(convolution);
            Pixel[,] imageContour = new Pixel[image.GetLength(0), image.GetLength(1)];
            for (int i = 0; i < hauteur; i++)
                for (int j = 0; j < largeur; j++)
                    imageContour[i, j] = new Pixel();

            //On initialise des valeurs provisoires
            int valeurProvisoireBleu = 0;
            int valeurProvisoireVert = 0;
            int valeurProvisoireRouge = 0;

            //On met les bords à 0
            for (int i = 0; i < imageContour.GetLength(1); i++)
            {
                imageContour[0, i].Bleu = 0;
                imageContour[0, i].Vert = 0;
                imageContour[0, i].Rouge = 0;

                imageContour[imageContour.GetLength(0) - 1, i].Bleu = 0;
                imageContour[imageContour.GetLength(0) - 1, i].Vert = 0;
                imageContour[imageContour.GetLength(0) - 1, i].Rouge = 0;
            }
            for (int j = 0; j < imageContour.GetLength(0); j++)
            {
                imageContour[j, 0].Bleu = 0;
                imageContour[j, 0].Vert = 0;
                imageContour[j, 0].Rouge = 0;

                imageContour[j, imageContour.GetLength(1) - 1].Bleu = 0;
                imageContour[j, imageContour.GetLength(1) - 1].Vert = 0;
                imageContour[j, imageContour.GetLength(1) - 1].Rouge = 0;
            }

            //On effectue les calculs pour le reste de la matrice
            for (int i = 1; i < (hauteur) - 1; i++)
            {
                for (int j = 3; j < (largeur ) - 1; j++)
                {
                    valeurProvisoireBleu = (image[i, j - 1].Bleu + image[i - 1, j].Bleu - (4 * image[i, j].Bleu) + image[i + 1, j].Bleu + image[i, j + 1].Bleu);
                    if (valeurProvisoireBleu < 0)
                        valeurProvisoireBleu = 0;
                    if (valeurProvisoireBleu > 255)
                        valeurProvisoireBleu = 255;

                    valeurProvisoireVert = (image[i, j - 1].Vert + image[i - 1, j].Vert - (4 * image[i, j].Vert) + image[i + 1, j].Vert + image[i, j + 1].Vert);
                    if (valeurProvisoireVert < 0)
                        valeurProvisoireVert = 0;
                    if (valeurProvisoireVert > 255)
                        valeurProvisoireVert = 255;

                    valeurProvisoireRouge = (image[i, j - 1].Rouge + image[i - 1, j].Rouge - (4 * image[i, j].Rouge) + image[i + 1, j].Rouge + image[i, j + 1].Rouge);
                    if (valeurProvisoireRouge < 0)
                        valeurProvisoireRouge = 0;
                    if (valeurProvisoireRouge > 255)
                        valeurProvisoireRouge = 255;
                    imageContour[i, j].Bleu = Convert.ToByte(valeurProvisoireBleu);
                    imageContour[i, j].Vert = Convert.ToByte(valeurProvisoireVert);
                    imageContour[i, j].Rouge = Convert.ToByte(valeurProvisoireRouge);
                }
            }
            image = imageContour;
        }
        /// <summary>
        /// Applique un effet de repoussage sur l'image
        /// </summary>
        public void ImageRepoussage()
        {
            //Matrice de convolution de repoussage
            int[,] convolution = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
            Convolution(convolution);
        }
        /// <summary>
        /// Matrice de convolution en entrée, cette méthode permet de lancer les calculs de chaque case
        /// </summary>
        /// <param name="convolution"></param>
        /// <param name="coef"></param>
        /// <param name="decalage"></param>
        public void Convolution(int[,] convolution, double coef = 1, int decalage = 1)
        {
            Pixel[,] newimage = new Pixel[image.GetLength(0), image.GetLength(1)];
            if (image != null && image.Length != 0 && convolution != null && convolution.Length != 0)
            {
                for (int l = 0; l < image.GetLength(0); l++)
                {
                    for (int c = 0; c < image.GetLength(1); c++)
                    {
                        Pixel newpixel = new()
                        {
                            Rouge = Convertir_Int_To_Endian2(SommeConvolution(convolution, l, c, coef, 1)),
                            Vert = Convertir_Int_To_Endian2(SommeConvolution(convolution, l, c, coef, 2)),
                            Bleu = Convertir_Int_To_Endian2(SommeConvolution(convolution, l, c, coef, 3))
                        };
                        newimage[l, c] = new Pixel()
                        {
                            Rouge = Convertir_Int_To_Endian2(newpixel.Rouge > 255 ? 255 : newpixel.Rouge),
                            Bleu = Convertir_Int_To_Endian2(newpixel.Bleu > 255 ? 255 : newpixel.Bleu),
                            Vert = Convertir_Int_To_Endian2(newpixel.Vert > 255 ? 255 : newpixel.Vert)
                        };
                    }
                }
            }
            image = newimage;
            
        }
        /// <summary>
        /// Uniquement appelée par la méthode de convolution pour faire les calculs de la matrice de convolution avec l'image
        /// </summary>
        /// <param name="convolution"></param>
        /// <param name="l"></param>
        /// <param name="c"></param>
        /// <param name="coef"></param>
        /// <param name="color"></param>
        /// <returns>somme du produit de chaque case autour de notre case avec la matrice de convolution</returns>
        public int SommeConvolution(int[,] convolution, int l, int c, double coef, int color)
        {
            double s = 0; //Somme
            for (int i = 0; i < convolution.GetLength(0); i++)
            {
                for (int j = 0; j < convolution.GetLength(1); j++)
                {
                    int x = i + (l - convolution.GetLength(0) / 2);
                    if (x < 0) //Permet de calculer avec le pixel du bord opposé quand on est trop proche de notre bord
                    {
                        x = image.GetLength(0) - x;
                    }
                    if (x >= image.GetLength(0))
                    {
                        x -= image.GetLength(0);
                    }
                    int y = j + (c - convolution.GetLength(1) / 2);
                    if (y < 0)
                    {
                        y = image.GetLength(1) - y;
                    }
                    if (y >= image.GetLength(1))
                    {
                        y -= image.GetLength(1);
                    }
                    switch (color)
                    {
                        case 1:
                            s += coef * image[x, y].Rouge * convolution[i, j];
                            break;
                        case 2:
                            s += coef * image[x, y].Vert * convolution[i, j];
                            break;
                        case 3:
                            s += coef * image[x, y].Bleu * convolution[i, j];
                            break;
                    }
                }
            }
            return (int) s;
        }
        /// <summary>
        /// Dessine une image fractale avec la précision demandée en paramètre
        /// </summary>
        /// <param name="valeur"></param>
        /// <param name="imagepixel"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        public void Fractale(int valeur, int[,] imagepixel, int x1 = 255, int x2 = 767, int y1 = 512, int y2 = 512)
        {
            int[,] pixelfractale = EquationFractale(valeur, x1, x2, y1, y2, imagepixel); //Matrice de int de mon image
            Pixel[,] imagefractale = new Pixel[pixelfractale.GetLength(0), pixelfractale.GetLength(1)]; //Matrice de Pixel de mon image

            //Remplissage de la matrice de Pixel avec les valeurs de la matrice de int (que l'on convertit en byte)
            for (int i = 0; i < pixelfractale.GetLength(0); i++)
            {
                for (int j = 0; j < pixelfractale.GetLength(1); j++)
                {
                    imagefractale[i, j] = new Pixel();
                    imagefractale[i, j].Bleu = Convertir_Int_To_Endian2(pixelfractale[i, j]);
                    imagefractale[i, j].Vert = Convertir_Int_To_Endian2(pixelfractale[i, j]);
                    imagefractale[i, j].Rouge = Convertir_Int_To_Endian2(pixelfractale[i, j]);
                }
            }

            //Création de l'entête de mon image
            byte[] entete = new byte[54];
            byte[] aide;
            for (int i = 0; i < entete.Length; i++) entete[i] = 0x00;

            entete[0] = 0x42;
            entete[1] = 0x4D;

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(0) * imagefractale.GetLength(1) * 24 + 54, 4);
            for (int i = 2; i < 6; i++) entete[i] = aide[i - 2];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(54, 4);
            for (int i = 10; i < 14; i++) entete[i] = aide[i - 10];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(40, 4);
            for (int i = 14; i < 18; i++) entete[i] = aide[i - 14];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(1), 4);
            for (int i = 18; i < 22; i++) entete[i] = aide[i - 18];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(0), 4);
            for (int i = 22; i < 26; i++) entete[i] = aide[i - 22];

            aide = new byte[2];
            aide = Convertir_Int_To_Endian(24, 4);
            for (int i = 28; i < 30; i++) entete[i] = aide[i - 28];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(0) * imagefractale.GetLength(1) * 24, 4);
            for (int i = 34; i < 38; i++) entete[i] = aide[i - 34];

            //Création du fichier
            byte[] imagebyte = new byte[54 + (imagefractale.GetLength(0) * 3 * imagefractale.GetLength(1))];
            for (int i = 0; i < 54; i++) imagebyte[i] = entete[i]; //Remplissage de l'entete

            //Remplissage du reste du fichier
            int a = 54;
            for (int i = 0; i < imagefractale.GetLength(0); i++)
            {
                for (int j = 0; j < imagefractale.GetLength(1); j++)
                {
                    imagebyte[a] = imagefractale[i, j].Bleu;
                    a++;
                    imagebyte[a] = imagefractale[i, j].Vert;
                    a++;
                    imagebyte[a] = imagefractale[i, j].Rouge;
                    a++;
                }
            }

            File.WriteAllBytes("ImageFractale.bmp", imagebyte);
        }
        /// <summary>
        /// Methode récursive qui répète n fois le calcul de la fractale (augmentant la précision de notre fractale à chaque fois)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public int[,] EquationFractale(int n, int x1, int x2, int y1, int y2, int[,] image)
        {
            int x3;
            int y3;
            if(n <= 1) DessinerUneLigne(x1, x2, y1, y2, image); //Une fois les calculs finis
            else //récursivité
            {
                x3 = (x1 + x2 + y2 - y1) / 2;
                y3 = (x1 - x2 + y1 + y2) / 2;
                EquationFractale(n - 1, x1, x3, y1, y3, image);
                EquationFractale(n - 1, x2, x3, y2, y3, image);
            }
            return image;
        }
        /// <summary>
        /// Dessigne la ligne de la fractale
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="imagepixel"></param>
        public void DessinerUneLigne(int x1, int x2, int y1, int y2, int[,] imagepixel)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int y = y1;
            int x = x1;
            int e = 0;

            //On vérifie que nos x1, x2, y1, y2 sont dans le bon ordre
            int ex = 1;
            int ey = 1;
            if (dx <= 0) ex = -1;
            if (dy <= 0) ey = -1;
            dx *= ex;
            dy *= ey;

            //On applique l'équation de notre fractale
            if (dx > dy)
            {
                e = dx / 2;
                for (int i = 0; i < dx; i++)
                {
                    e += dy;
                    if (e >= dx)
                    {
                        y += ey;
                        e -= dx;
                    }
                    x += ex;
                    imagepixel[y, x] = 255;
                }
            }
            else //(dx <= dy)
            {
                e = ey / 2;
                for (int i = 0; i < dy; i++)
                {
                    e += dx;
                    if (e >= dy)
                    {
                        x += ex;
                        e -= dy;
                    }
                    y += ey;
                    imagepixel[y, x] = 255;
                }
            }
        }
        /// <summary>
        /// Donne le nombre de pixel de notre image dans chaque gamme de couleur
        /// </summary>
        public void HistogrammeDonnees()
        {
            //Initialisations de nos compteurs de chaque catégorie
            int bleumin = 0;
            int bleubas = 0;
            int bleuhaut = 0;
            int bleumax = 0;

            int vertmin = 0;
            int vertbas = 0;
            int verthaut = 0;
            int vertmax = 0;

            int rougemin = 0;
            int rougebas = 0;
            int rougehaut = 0;
            int rougemax = 0;

            //Pour chaque pixel RGB, les compteurs prennent +1 si la couleur est dans leur gamme
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    if (image[i, j].Bleu < 64) bleumin += 1;
                    if (image[i, j].Bleu >= 64 && image[i, j].Bleu < 128) bleubas += 1;
                    if (image[i, j].Bleu >= 128 && image[i, j].Bleu < 192) bleuhaut += 1;
                    if (image[i, j].Bleu >= 192) bleumax += 1;

                    if (image[i, j].Vert < 64) vertmin += 1;
                    if (image[i, j].Vert >= 64 && image[i, j].Vert < 128) vertbas += 1;
                    if (image[i, j].Vert >= 128 && image[i, j].Vert < 192) verthaut += 1;
                    if (image[i, j].Vert >= 192) vertmax += 1;

                    if (image[i, j].Rouge < 64) rougemin += 1;
                    if (image[i, j].Rouge >= 64 && image[i, j].Rouge < 128) rougebas += 1;
                    if (image[i, j].Rouge >= 128 && image[i, j].Rouge < 192) rougehaut += 1;
                    if (image[i, j].Rouge >= 192) rougemax += 1;
                }
            }

            //Affichage tout bête du résultat
            Console.WriteLine(
                "\nBleu 0 à 63 : " + bleumin + "\n" +
                "Bleu 64 à 127 : " + bleubas + "\n" +
                "Bleu 128 à 191 : " + bleuhaut + "\n" +
                "Bleu 192 à 255 : " + bleumax + "\n\n" +

                "Vert 0 à 63 : " + vertmin + "\n" +
                "Vert 64 à 127 : " + vertbas + "\n" +
                "Vert 128 à 191 : " + verthaut + "\n" +
                "Vert 192 à 255 : " + vertmax + "\n\n" +

                "Rouge 0 à 63 : " + rougemin + "\n" +
                "Rouge 64 à 127 : " + rougebas + "\n" +
                "Rouge 128 à 191 : " + rougehaut + "\n" +
                "Rouge 192 à 255 : " + rougemax             );
        }
        /// <summary>
        /// Réalise un histogramme dans chaque couleur des couleurs présentes dans notre image
        /// </summary>
        public void HistogrammeImage()
        {
            //Initialisation de tableaux qui permettent de récupérer la donnée de chaque couleur
            int[] bleu = new int[256];
            int[] vert = new int[256];
            int[] rouge = new int[256];

            //On met tout à 0
            for (int i = 0; i < 256; i++)
            {
                bleu[i] = 0;
                vert[i] = 0;
                rouge[i] = 0;
            }

            //On met +1 à chaque fois que la valeur du pixel est dans son numéro (on teste de 0 à 255 à chaque fois, pour chaque couleur et chaque case)
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    for (int k = 0; k < 256; k++)
                    {
                        if (k == image[i, j].Bleu) bleu[k]++;
                        if (k == image[i, j].Vert) vert[k]++;
                        if (k == image[i, j].Rouge) rouge[k]++;
                    }
                }
            }

            //On regarde jusqu'où ça monte afin de pouvoir créer une matrice de pixel de la bonne taille
            int maxbleu = Max(bleu);
            int maxvert = Max(vert);
            int maxrouge = Max(rouge);

            //Création des matrices pour chaque couleur
            Pixel[,] imagebleu = new Pixel[maxbleu, 256 * 9]; //Le *9 a juste pour objectif d'avoir une meilleur lisibilité de la donnée
            Pixel[,] imagevert = new Pixel[maxvert, 256 * 9];
            Pixel[,] imagerouge = new Pixel[maxrouge, 256 * 9];

            //On initialise tous les pixels, on en profite pour mettre les pixels des couleurs qu'on ne va pas utiliser à 0
            for (int j = 0; j < imagebleu.GetLength(1); j++)
            {
                for (int i = 0; i < imagebleu.GetLength(0); i++)
                {
                    imagebleu[i, j] = new Pixel();
                    imagebleu[i, j].Bleu = 0;
                    imagebleu[i, j].Vert = 0;
                    imagebleu[i, j].Rouge = 0;
                }
                for (int i = 0; i < imagevert.GetLength(0); i++)
                {
                    imagevert[i, j] = new Pixel();
                    imagevert[i, j].Vert = 0;
                    imagevert[i, j].Bleu = 0;
                    imagevert[i, j].Rouge = 0;
                }
                for (int i = 0; i < imagerouge.GetLength(0); i++)
                {
                    imagerouge[i, j] = new Pixel();
                    imagerouge[i, j].Rouge = 0;
                    imagerouge[i, j].Bleu = 0;
                    imagerouge[i, j].Vert = 0;
                }
            }

            //On remplit les cases de chaque pixel au nombre auquel il doit se trouver
            for (int j = 0; j < imagebleu.GetLength(1); j++)
            {
                for (int i = 0; i < bleu[j / 9]; i++) imagebleu[i, j].Bleu = 255;
                for (int i = 0; i < vert[j / 9]; i++) imagevert[i, j].Vert = 255;
                for (int i = 0; i < rouge[j / 9]; i++) imagerouge[i, j].Rouge = 255;
            }

            //Création des entêtes de chacun des histogrammes
            byte[] entetebleu = new byte[54];
            entetebleu = NouvelEntete(imagebleu);

            byte[] entetevert = new byte[54];
            entetevert = NouvelEntete(imagevert);

            byte[] enteterouge = new byte[54];
            enteterouge = NouvelEntete(imagerouge);

            //Création d'un tableau de byte, le fichier final
            byte[] bytebleu = new byte[54 + imagebleu.GetLength(0) * imagebleu.GetLength(1) * 3];
            byte[] bytevert = new byte[54 + imagevert.GetLength(0) * imagevert.GetLength(1) * 3];
            byte[] byterouge = new byte[54 + imagerouge.GetLength(0) * imagerouge.GetLength(1) * 3];

            //On remplit les entêtes
            for(int i = 0; i < 54; i++)
            {
                bytebleu[i] = entetebleu[i];
                bytevert[i] = entetevert[i];
                byterouge[i] = enteterouge[i];
            }

            //On remplit les fichiers
            int a = 54;
            for (int i = 0; i < imagebleu.GetLength(0); i++)
                for (int j = 0; j < imagebleu.GetLength(1); j = j + 3)
                {
                    bytebleu[a] = imagebleu[i, j].Bleu;
                    a = a + 3;
                }

            File.WriteAllBytes("HistoBleu.bmp", bytebleu); //Enregistrement du fichier

            a = 54;
            for (int i = 0; i < imagevert.GetLength(0); i++)
                for (int j = 0; j < imagevert.GetLength(1); j = j + 3)
                {
                    bytevert[a + 1] = imagevert[i, j].Vert;
                    a = a + 3;
                }

            File.WriteAllBytes("HistoVert.bmp", bytevert); //Enregistrement du fichier

            a = 54;
            for (int i = 0; i < imagerouge.GetLength(0); i++)
                for (int j = 0; j < imagerouge.GetLength(1); j = j + 3)
                {
                    byterouge[a + 2] = imagerouge[i, j].Rouge;
                    a = a + 3; ;
                }

            File.WriteAllBytes("HistoRouge.bmp", byterouge); //Enregistrement du fichier
        }
        /// <summary>
        /// Permet de trouver le maximum d'un tableau donné
        /// </summary>
        /// <param name="tab"></param>
        /// <returns>int max : maximum du tableau</returns>
        public int Max(int[] tab)
        {
            int max = 0;
            foreach (int element in tab) 
                if (max < element) max = element;

            if (max % 4 != 0) 
                while (max % 4 != 0) max++;

            return max;
        }
        /// <summary>
        /// Permet de créer une entête pour une matrice de pixel donnée
        /// </summary>
        /// <param name="imagefractale"></param>
        /// <returns>byte[] entete : entete du document</returns>
        public byte[] NouvelEntete(Pixel[,] imagefractale)
        {
            byte[] entete = new byte[54];
            byte[] aide;
            for (int i = 0; i < entete.Length; i++) entete[i] = 0x00;

            entete[0] = 0x42;
            entete[1] = 0x4D;

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(0) * imagefractale.GetLength(1) * 24 + 54, 4);
            for (int i = 2; i < 6; i++) entete[i] = aide[i - 2];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(54, 4);
            for (int i = 10; i < 14; i++) entete[i] = aide[i - 10];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(40, 4);
            for (int i = 14; i < 18; i++) entete[i] = aide[i - 14];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(1), 4);
            for (int i = 18; i < 22; i++) entete[i] = aide[i - 18];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(0), 4);
            for (int i = 22; i < 26; i++) entete[i] = aide[i - 22];

            aide = new byte[2];
            aide = Convertir_Int_To_Endian(24, 4);
            for (int i = 28; i < 30; i++) entete[i] = aide[i - 28];

            aide = new byte[4];
            aide = Convertir_Int_To_Endian(imagefractale.GetLength(0) * imagefractale.GetLength(1) * 24, 4);
            for (int i = 34; i < 38; i++) entete[i] = aide[i - 34];

            return entete;
        }
        /// <summary>
        /// Permet de "cacher" une image dans une autre image
        /// </summary>
        /// <param name="image2"></param>
        public void CoderImage(Pixel[,] image2)
        {
            //Vérification de la taille des deux images (elle doit être équivalente)
            if (image.GetLength(0) != image2.GetLength(0) || image.GetLength(1) != image2.GetLength(1))
                Console.WriteLine("Les deux images doivent être de la même taille afin de réaliser cette opération");

            else
            {
                Pixel[,] newimage = new Pixel[image.GetLength(0), image.GetLength(1)];
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        newimage[i, j] = new Pixel();
                        newimage[i, j].Bleu = Convert.ToByte(image[i, j].Bleu / 16 * 16 + image2[i, j].Bleu / 16);
                        newimage[i, j].Vert = Convert.ToByte(image[i, j].Vert / 16 * 16 + image2[i, j].Vert / 16);
                        newimage[i, j].Rouge = Convert.ToByte(image[i, j].Rouge / 16 * 16 + image2[i, j].Rouge / 16);
                    }
                }
                image = newimage;
            }
        }
        /// <summary>
        /// Decode l'image de la class en deux images différentes (marche uniquement avec une image composée de deux autres images codées ensemble)
        /// </summary>
        public void DecoderImage()
        {
            Pixel[,] nouvelleimage = new Pixel[hauteur, largeur];
            Pixel[,] ancienneimage = new Pixel[hauteur, largeur];

            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    nouvelleimage[i, j] = new Pixel();
                    ancienneimage[i, j] = new Pixel();

                    //Récupération de l'image qui apparait à l'écran
                    ancienneimage[i, j].Bleu = Convert.ToByte((image[i, j].Bleu / 16) * 16);
                    ancienneimage[i, j].Vert = Convert.ToByte((image[i, j].Vert / 16) * 16);
                    ancienneimage[i, j].Rouge = Convert.ToByte((image[i, j].Rouge / 16) * 16);

                    //Récupération de l'image codée
                    nouvelleimage[i, j].Bleu = Convert.ToByte((image[i, j].Bleu % 16) * 16);
                    nouvelleimage[i, j].Vert = Convert.ToByte((image[i, j].Vert % 16) * 16);
                    nouvelleimage[i, j].Rouge = Convert.ToByte((image[i, j].Rouge % 16) * 16);
                }
            }

            //Sauvegarde des images dans des fichiers

            //Déclaration des variables
            byte[] newbyte = new byte[54 + hauteur * largeur * 3];
            byte[] oldbyte = new byte[54 + hauteur * largeur * 3];
            byte[] newentete = new byte[54];
            byte[] oldentete = new byte[54];

            //Création des entetes
            newentete = NouvelEntete(nouvelleimage);
            oldentete = NouvelEntete(ancienneimage);

            //Remplissage des entêtes des nouveaux fichiers
            for (int i = 0; i < 54; i++)
            {
                newbyte[i] = newentete[i];
                oldbyte[i] = oldentete[i];
            }

            int a = 54; //Position dans le fichier
            //Remplissage du fichier à partir de l'emplacement 54
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    newbyte[a] = nouvelleimage[i, j].Bleu;
                    newbyte[a + 1] = nouvelleimage[i, j].Vert;
                    newbyte[a + 2] = nouvelleimage[i, j].Rouge;

                    oldbyte[a] = ancienneimage[i, j].Bleu;
                    oldbyte[a + 1] = ancienneimage[i, j].Vert;
                    oldbyte[a + 2] = ancienneimage[i, j].Rouge;

                    a = a + 3;
                }
            }

            //Enregistrement des deux images
            File.WriteAllBytes("ImageEvidente.bmp", oldbyte); 
            File.WriteAllBytes("ImageCachee.bmp", newbyte); 
        }
        /// <summary>
        /// Transforme une chaine de caractères en QRCode
        /// </summary>
        public void QRCode()
        {
            //On demande à l'utilisateur la phrase qu'il compte enregister et on l'enregistre ensuite en string
            Console.WriteLine("Bonjour, merci de taper la chaîne de caractères que vous voulez transformer en QRCode (47 caractères max)");
            string chaine = Convert.ToString(Console.ReadLine()).ToUpper(); //On met directement le résultat en majuscule car nous sommes en alphanumérique
            
            //On vérifie qu'elle est uniquement composée de caractères que l'on a le droit d'utiliser
            bool ret;
            do
            {
                ret = true;
                for (int i = 0; i < chaine.Length; i++)
                    if (!char.IsLetterOrDigit(chaine[i]))
                        if ((chaine[i] != '$' && chaine[i] != ' ' && chaine[i] != '/' && chaine[i] != '.' && chaine[i] != '-' && chaine[i] != '+' && chaine[i] != '%' && chaine[i] != '*' && chaine[i] != ':') || chaine.Length > 47)
                        {
                            ret = false;
                            Console.WriteLine("La chaîne doit être uniquement composée de caractères alphanumériques, merci de redonner une nouvelle chaine (47 caractères max)\n");
                            chaine = Convert.ToString(Console.ReadLine()).ToUpper();
                        }
            } while (!ret);

            //On convertit désormais notre chaine de caractères en tableau de caractères
            char[] tabchaine = new char[chaine.Length];
            for (int i = 0; i < chaine.Length; i++) tabchaine[i] = chaine[i]; //Ainsi chaque caractère de notre chaine est dans une case de notre tableau

            //Choix de la version
            bool clav1 = true; //Version 1
            if (chaine.Length > 25) clav1 = false; //Version2

            //Indicateur du mode sur 4 bits
            int[] mode = { 0, 0, 1, 0 };

            //On enregistre la taille de notre chaine de caractère
            int[] taille = ConvertEnBits(chaine.Length, 9);

            //Maintenant on souhaite convertir cette chaîne en nombre utilisables, donc int, j'ai créé CharEnAlphaNum pour ça
            int[] tabnum = new int[chaine.Length];
            for (int i = 0; i < chaine.Length; i++) tabnum[i] = CharEnAlphaNum(tabchaine[i]);

            //On regarde si le nombre de caractères est pair
            bool EstPair = true; //pair
            if (chaine.Length % 2 != 0) EstPair = false; //impair

            //On cherche maintenant à faire le calcul de bits
            int nbPaires = chaine.Length / 2;
            int[] donnees;
            if (EstPair) donnees = new int[nbPaires * 11];
            else { donnees = new int[nbPaires * 11 + 6]; }
            for (int i = 0; i < donnees.Length; i++) donnees[i] = 0;

            //On remplit la variable données par les données de la chaine de caractère en bits
            for (int i = 0; i < nbPaires; i++)
            {
                int[] temporaire = ConvertEnBits(tabnum[2 * i] * 45 + tabnum[2 * i + 1], 11);
                for (int j = 0; j < temporaire.Length; j++) donnees[i * 11 + j] = temporaire[j];
            }
            if (!EstPair) //Remplissage des 6 derniers bits si la chaine est impaire
            {
                int[] blablou = ConvertEnBits(tabnum[tabnum.Length - 1], 6);
                for (int j = 0; j < blablou.Length; j++) donnees[nbPaires * 11 + j] = blablou[j];
            }

            //On ajoute désormais les bits de taille et de mode qu'on a déjà enregistré au préalable
            int[] tempo = new int[donnees.Length + taille.Length + mode.Length];
            for (int i = 0; i < mode.Length; i++) tempo[i] = mode[i]; //On met les bits du mode
            for (int i = mode.Length; i < mode.Length + taille.Length; i++) tempo[i] = taille[i - mode.Length]; //Puis ceux de la taille
            for (int i = mode.Length + taille.Length; i < tempo.Length; i++) tempo[i] = donnees[i - mode.Length - taille.Length]; //Puis on rajoute les données
            donnees = tempo; //Et on remet tout ça dans données

            //On rajoute désormais tous les bits qui nous sont demandés pour compléter les données
            if (clav1) //Ici en version 1
            {
                int count = 0;
                while(donnees.Length < 152 && count < 4) //Les quatre 0 pour commencer
                {
                    int[] temporaire = new int[donnees.Length + 1];
                    for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                    temporaire[temporaire.Length - 1] = 0;
                    donnees = temporaire;
                    count++;
                }
                while(donnees.Length % 8 != 0) //Puis les 0 pour être multiple de 8
                {
                    int[] temporaire = new int[donnees.Length + 1];
                    for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                    temporaire[temporaire.Length - 1] = 0;
                    donnees = temporaire;
                }
                while (donnees.Length < 152) //Puis les 16 bits pour compléter
                {
                    int[] temporaire = new int[donnees.Length + 8];
                    int[] temporaire2 = { 1,1,1,0,1,1,0,0};
                    for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                    for (int i = 0; i < temporaire2.Length; i++) temporaire[donnees.Length + i] = temporaire2[i];
                    donnees = temporaire;

                    if (donnees.Length < 152) //On vérifie juste bien qu'on est pas arrivé au compte à 8 et non à 16
                    {
                        temporaire = new int[donnees.Length + 8];
                        temporaire2 = new int[] { 0, 0, 0, 1, 0, 0, 0, 1 };
                        for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                        for (int i = 0; i < temporaire2.Length; i++) temporaire[donnees.Length + i] = temporaire2[i];
                        donnees = temporaire;
                    }
                }
            }
            else //Ici en version 2, les commentaires sont les mêmes c'est juste 272 de taille à atteindre et non 152
            {
                int count = 0;
                while (donnees.Length < 272 && count < 4)
                {
                    int[] temporaire = new int[donnees.Length + 1];
                    for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                    temporaire[temporaire.Length - 1] = 0;
                    donnees = temporaire;
                    count++;
                }
                while (donnees.Length % 8 != 0)
                {
                    int[] temporaire = new int[donnees.Length + 1];
                    for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                    temporaire[temporaire.Length - 1] = 0;
                    donnees = temporaire;
                }
                while (donnees.Length < 272)
                {
                    int[] temporaire = new int[donnees.Length + 8];
                    int[] temporaire2 = { 1, 1, 1, 0, 1, 1, 0, 0 };
                    for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                    for (int i = 0; i < temporaire2.Length; i++) temporaire[donnees.Length + i] = temporaire2[i];
                    donnees = temporaire;

                    while (donnees.Length < 272)
                    {
                        temporaire = new int[donnees.Length + 8];
                        temporaire2 = new int[] { 0, 0, 0, 1, 0, 0, 0, 1 };
                        for (int i = 0; i < donnees.Length; i++) temporaire[i] = donnees[i];
                        for (int i = 0; i < temporaire2.Length; i++) temporaire[donnees.Length + i] = temporaire2[i];
                        donnees = temporaire;
                    }
                }
            }

            byte[] bytesa; //Création de notre variable byte[] qui va prendre nos données
            byte[] result; //Création d'une variable result qui prendra en paramètre la correction d'erreur

            //Correction d'erreur
            if (clav1)  //Version 1
            {
                bytesa = new byte[19]; //Taille de 19 octets en version 1
                int[] temporaire = new int[8]; //Création d'un int[] qui prendra chaque octet (huit 0 ou 1 en valeur afin de le convertir ensuite)
                for(int i = 0; i < 19; i++) //On remplit bytesa avec les données
                {
                    for (int j = 0; j < 8; j++) temporaire[j] = donnees[i * 8 + j];
                    bytesa[i] = ConvertirEnByte(temporaire);
                }
                result = ReedSolomonAlgorithm.Encode(bytesa, 7, ErrorCorrectionCodeType.QRCode); //On applique la méthode de correction d'erreur

                //On ajoute les éléments de correction à notre byte[]
                byte[] temporaire2 = new byte[result.Length + bytesa.Length];
                for (int i = 0; i < bytesa.Length; i++) temporaire2[i] = bytesa[i];
                for (int i = 0; i < result.Length; i++) temporaire2[bytesa.Length + i] = result[i];
                bytesa = temporaire2;
            }
            else //Version 2
            {
                bytesa = new byte[34]; //Taille de 34 octets en version 2
                int[] temporaire = new int[8]; //Création d'un int[] qui prendra chaque octet (huit 0 ou 1 en valeur afin de le convertir ensuite)
                for (int i = 0; i < 34; i++) //On remplit bytesa avec les données
                {
                    for (int j = 0; j < 8; j++) temporaire[j] = donnees[i * 8 + j];
                    bytesa[i] = ConvertirEnByte(temporaire);
                }
                result = ReedSolomonAlgorithm.Encode(bytesa, 10, ErrorCorrectionCodeType.QRCode); //On applique la méthode de correction d'erreur

                //On ajoute les éléments de correction à notre byte[]
                byte[] temporaire2 = new byte[result.Length + bytesa.Length];
                for (int i = 0; i < bytesa.Length; i++) temporaire2[i] = bytesa[i];
                for (int i = 0; i < result.Length; i++) temporaire2[bytesa.Length + i] = result[i];
                bytesa = temporaire2;
            }

            //On repasse notre donnée dans un tableau de 0 et de 1 pour se simplifier la vie
            int[] final = new int[bytesa.Length * 8];
            for (int i = 0; i < bytesa.Length; i++)
            {
                int[] temporaire = ConvertEnBits(Convert.ToInt32(bytesa[i]), 8);
                for (int j = 0; j < 8; j++) final[i * 8 + j] = temporaire[j];
            }

            int[] mask = { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
            int[,] QRCode = CreerQRCode(clav1); //Création de la base du QRCode

            //Maintenant, on passe à la création du QR Code
            if (clav1) //Version 1
            {
                //Remplissage de la donnée, zone par zone

                for(int i = 0; i < 4; i++)
                    for(int j = 0; j < 12; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[QRCode.GetLength(0) - 1 - j, QRCode.GetLength(1) - 1 - k - 2 * i] = final[k + 2 * j + 24 * i]; //Quand ça monte
                                if((QRCode.GetLength(0) - 1 - j + QRCode.GetLength(1) - 1 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[k + 2 * j + 24 * i] == 0) QRCode[QRCode.GetLength(0) - 1 - j, QRCode.GetLength(1) - 1 - k - 2 * i] = 1;
                                    else { QRCode[QRCode.GetLength(0) - 1 - j, QRCode.GetLength(1) - 1 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            { 
                                QRCode[9 + j, QRCode.GetLength(1) - 1 - k - 2 * i] = final[k + 2 * j + 24 * i];
                                if((9 + j + QRCode.GetLength(1) - 1 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[k + 2 * j + 24 * i] == 0) QRCode[9 + j, QRCode.GetLength(1) - 1 - k - 2 * i] = 1;
                                    else { QRCode[9 + j, QRCode.GetLength(1) - 1 - k - 2 * i] = 0; }
                                }
                            } //Quand ça descend
                        } //96 blocs

                for (int i = 0; i < 14; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[QRCode.GetLength(0) - i - 1, QRCode.GetLength(1) - 9 - j] = final[96 + j + 2 * i]; //124 blocs
                        if((QRCode.GetLength(0) - i - 1 + QRCode.GetLength(1) - 9 - j) % 2 == 0)
                        {
                            if (final[96 + j + 2 * i] == 0) QRCode[QRCode.GetLength(0) - i - 1, QRCode.GetLength(1) - 9 - j] = 1;
                            else { QRCode[QRCode.GetLength(0) - i - 1, QRCode.GetLength(1) - 9 - j] = 0; }
                        }
                    }
                        

                for(int i = 0; i < 2; i++)
                    for(int j = 0; j < 6; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[5 - j, 12 - k - 2 * i] = final[124 + k + 2 * j + 12 * i];
                                if((5 - j + 12 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[124 + k + 2 * j + 12 * i] == 0) QRCode[5 - j, 12 - k - 2 * i] = 1;
                                    else { QRCode[5 - j, 12 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            { 
                                QRCode[j, 12 - k - 2 * i] = final[124 + k + 2 * j + 12 * i];
                                if ((j + 12 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[124 + k + 2 * j + 12 * i] == 0) QRCode[j, 12 - k - 2 * i] = 1;
                                    else { QRCode[j, 12 - k - 2 * i] = 0; }
                                }
                            }
                        } //148

                for (int i = 0; i < 14; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[7 + i, 10 - j] = final[148 + j + 2 * i]; //176
                        if((7 + i + 10 - j) % 2 == 0) //Masque
                        {
                            if (final[148 + j + 2 * i] == 0) QRCode[7 + i, 10 - j] = 1;
                            else { QRCode[7 + i, 10 - j] = 0; }
                        }
                    }

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[12 - i, 8 - j] = final[176 + j + 2 * i]; //184
                        if((12 - i + 8 - j) % 2 == 0) //Masque
                        {
                            if (final[176 + j + 2 * i] == 0) QRCode[12 - i, 8 - j] = 1;
                            else { QRCode[12 - i, 8 - j] = 0; }
                        }
                    }

                for (int i = 0; i < 3; i++)
                    for(int j = 0; j < 4; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[9 + j, 5 - k - 2 * i] = final[184 + k + 2 * j + 8 * i];
                                if((9 + j + 5 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[184 + k + 2 * j + 8 * i] == 0) QRCode[9 + j, 5 - k - 2 * i] = 1;
                                    else { QRCode[9 + j, 5 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            {
                                QRCode[12 - j, 5 - k - 2 * i] = final[184 + k + 2 * j + 8 * i];  
                                if((12 - j + 5 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[184 + k + 2 * j + 8 * i] == 0) QRCode[12 - j, 5 - k - 2 * i] = 1;
                                    else { QRCode[12 - j, 5 - k - 2 * i] = 0; }
                                }
                            }
                        } //208 blocs

                //Puis les bits de correcteur et de masque, à placer dans les cases bleues
                for (int i = 0; i < 7; i++) QRCode[QRCode.GetLength(0) - 1 - i, 8] = mask[i]; //Bas à gauche 0 à 6
                for (int i = 0; i < 8; i++) QRCode[8, 13 + i] = mask[7 + i]; //Haut à droite 7 à 14

                for (int i = 0; i < 6; i++) QRCode[8, i] = mask[i];
                for (int i = 0; i < 2; i++) QRCode[8, 7 + i] = mask[6 + i];
                for (int i = 0; i < 7; i++) QRCode[6 - i, 8] = mask[8 + i];
                //Fin de matrice d'entiers 0 ou 1 QR Code
            }
            else //Version 2
            {
                for(int i = 0; i < 2; i++)
                    for(int j = 0; j < 16; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[QRCode.GetLength(0) - 1 - j, QRCode.GetLength(1) - 1 - k - 2 * i] = final[k + 2 * j + 32 * i];
                                if((QRCode.GetLength(0) - 1 - j + QRCode.GetLength(1) - 1 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[k + 2 * j + 32 * i] == 0) QRCode[QRCode.GetLength(0) - 1 - j, QRCode.GetLength(1) - 1 - k - 2 * i] = 1;
                                    else { QRCode[QRCode.GetLength(0) - 1 - j, QRCode.GetLength(1) - 1 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            { 
                                QRCode[9 + j, QRCode.GetLength(1) - 1 - k - 2 * i] = final[k + 2 * j + 32 * i]; 
                                if((9 + j + QRCode.GetLength(1) - 1 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[k + 2 * j + 32 * i] == 0) QRCode[9 + j, QRCode.GetLength(1) - 1 - k - 2 * i] = 1;
                                    else { QRCode[9 + j, QRCode.GetLength(1) - 1 - k - 2 * i] = 0; }
                                }
                            }
                        } //64 blocs

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 5 - j] = final[64 + j + 2 * i]; //72 blocs
                        if((QRCode.GetLength(0) - 1 - i + QRCode.GetLength(1) - 5 - j) % 2 == 0) //Masque
                        {
                            if (final[64 + j + 2 * i] == 0) QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 5 - j] = 1;
                            else { QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 5 - j] = 0; }
                        }
                    }

                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 7; j++)
                        for (int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[QRCode.GetLength(0) - 10 - j, QRCode.GetLength(1) - 5 - k - i * 2] = final[72 + k + 2 * j + 14 * i];
                                if ((QRCode.GetLength(0) - 10 - j + QRCode.GetLength(1) - 5 - k - i * 2) % 2 == 0) //Masque
                                {
                                    if (final[72 + k + 2 * j + 14 * i] == 0) QRCode[QRCode.GetLength(0) - 10 - j, QRCode.GetLength(1) - 5 - k - i * 2] = 1;
                                    else { QRCode[QRCode.GetLength(0) - 10 - j, QRCode.GetLength(1) - 5 - k - i * 2] = 0; }
                                }
                            }
                            else 
                            {
                                QRCode[9 + j, QRCode.GetLength(1) - 5 - k - i * 2] = final[72 + k + 2 * j + 14 * i]; 
                                if((9 + j + QRCode.GetLength(1) - 5 - k - i * 2) % 2 == 0) //Masque
                                {
                                    if (final[72 + k + 2 * j + 14 * i] == 0) QRCode[9 + j, QRCode.GetLength(1) - 5 - k - i * 2] = 1;
                                    else { QRCode[9 + j, QRCode.GetLength(1) - 5 - k - i * 2] = 0; }
                                }
                            }
                        } //100 blocs

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[21 + i, 18 - j] = final[100 + j + 2 * i]; //108 blocs
                        if((21 + i + 18 - j) % 2 == 0) //Masque
                        {
                            if (final[100 + j + 2 * i] == 0) QRCode[21 + i, 18 - j] = 1;
                            else { QRCode[21 + i, 18 - j] = 0; }
                        }
                    }

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[24 - i, 16 - j] = final[108 + j + 2 * i]; //116 blocs
                        if((24 - i + 16 - j) % 2 == 0) //Masque
                        {
                            if (final[108 + j + 2 * i] == 0) QRCode[24 - i, 16 - j] = 1;
                            else { QRCode[24 - i, 16 - j] = 0; }
                        }
                    }

                for (int i = 0; i < 5; i++)
                {
                    QRCode[20 - i, 15] = final[116 + i]; //121 blocs
                    if((20 - i + 15) % 2 == 0) //Masque
                    {
                        if (final[116 + i] == 0) QRCode[20 - i, 15] = 1;
                        else { QRCode[20 - i, 15] = 0; }
                    }
                }

                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[15 - i, 16 - j] = final[121 + j + 2 * i]; //139 blocs
                        if((15 - i + 16 - j) % 2 == 0) //Masque
                        {
                            if (final[121 + j + 2 * i] == 0) QRCode[15 - i, 16 - j] = 1;
                            else { QRCode[15 - i, 16 - j] = 0; }
                        }
                    }

                for(int i = 0; i < 2; i++)
                    for(int j = 0; j < 6; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[5 - j, 16 - k - 2 * i] = final[139 + k + 2 * j + 12 * i];
                                if ((5 - j + 16 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[139 + k + 2 * j + 12 * i] == 0) QRCode[5 - j, 16 - k - 2 * i] = 1;
                                    else { QRCode[5 - j, 16 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            {
                                QRCode[j, 16 - k - 2 * i] = final[139 + k + 2 * j + 12 * i]; 
                                if((j + 16 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[139 + k + 2 * j + 12 * i] == 0) QRCode[j, 16 - k - 2 * i] = 1;
                                    else { QRCode[j, 16 - k - 2 * i] = 0; }
                                }
                            }
                        } //163 blocs

                for(int i = 0; i < 2; i++)
                    for(int j = 0; j < 18; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[7 + j, 14 - k - 2 * i] = final[163 + k + 2 * j + 36 * i];
                                if((7 + j + 14 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[163 + k + 2 * j + 36 * i] == 0) QRCode[7 + j, 14 - k - 2 * i] = 1;
                                    else { QRCode[7 + j, 14 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            {
                                QRCode[24 - j, 14 - k - 2 * i] = final[163 + k + 2 * j + 36 * i]; 
                                if((24 - j + 14 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[163 + k + 2 * j + 36 * i] == 0) QRCode[24 - j, 14 - k - 2 * i] = 1;
                                    else { QRCode[24 - j, 14 - k - 2 * i] = 0; }
                                }
                            }
                        } //235 blocs

                for(int i = 0; i < 2; i++)
                    for(int j = 0; j < 6; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if (i % 2 == 0)
                            {
                                QRCode[5 - j, 12 - k - 2 * i] = final[235 + k + 2 * j + 12 * i];
                                if((5 - j + 12 - k - 2 * i) % 2 == 0) //Maque
                                {
                                    if (final[235 + k + 2 * j + 12 * i] == 0) QRCode[5 - j, 12 - k - 2 * i] = 1;
                                    else { QRCode[5 - j, 12 - k - 2 * i] = 0; }
                                }
                            }
                            else 
                            {
                                QRCode[j, 12 - k - 2 * i] = final[235 + k + 2 * j + 12 * i]; 
                                if((j + 12 - k - 2 * i) % 2 == 0) //Masque
                                {
                                    if (final[235 + k + 2 * j + 12 * i] == 0) QRCode[j, 12 - k - 2 * i] = 1;
                                    else { QRCode[j, 12 - k - 2 * i] = 1; }
                                }
                            }
                        } //259 blocs

                for (int i = 0; i < 18; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[7 + i, 10 - j] = final[259 + j + 2 * i]; //295 blocs
                        if ((7 + i + 10 - j) % 2 == 0) //Masque
                        {
                            if (final[259 + j + 2 * i] == 0) QRCode[7 + i, 10 - j] = 1;
                            else { QRCode[7 + i, 10 - j] = 0; }
                        }
                    }

                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        QRCode[16 - i, 8 - j] = final[295 + j + 2 * i]; //311 blocs
                        if((16 - i + 8 - j) % 2 == 0) //Masque
                        {
                            if (final[295 + j + 2 * i] == 0) QRCode[16 - i, 8 - j] = 1;
                            else { QRCode[16 - i, 8 - j] = 0; }
                        }
                    }

                for(int i = 0; i < 3; i++)
                    for(int j = 0; j < 8; j++)
                        for(int k = 0; k < 2; k++)
                        {
                            if(311 + k + 2 * j + 16 * i < 352) //Pour ne pas dépasser 352 (taille de final)
                            {
                                if (i % 2 == 0)
                                {
                                    QRCode[9 + j, 5 - k - 2 * i] = final[311 + k + 2 * j + 16 * i];
                                    if((9 + j + 5 - k - 2 * i) % 2 == 0) //Masque
                                    {
                                        if (final[311 + k + 2 * j + 16 * i] == 0) QRCode[9 + j, 5 - k - 2 * i] = 1;
                                        else { QRCode[9 + j, 5 - k - 2 * i] = 0; }
                                    }
                                }
                                else 
                                {
                                    QRCode[16 - j, 5 - k - 2 * i] = final[311 + k + 2 * j + 16 * i]; 
                                    if((16 - j + 5 - k - 2 * i) % 2 == 0) //Masque
                                    {
                                        if (final[311 + k + 2 * j + 16 * i] == 0) QRCode[16 - j, 5 - k - 2 * i] = 1;
                                        else { QRCode[16 - j, 5 - k - 2 * i] = 0; }
                                    }
                                }
                            }
                            else //On met des 0 dans les 7 cases restantes
                            {
                                if (i % 2 == 0)
                                {
                                    QRCode[9 + j, 5 - k - 2 * i] = 0;
                                    if ((9 + j + 5 - k - 2 * i) % 2 == 0) //Masque
                                        QRCode[9 + j, 5 - k - 2 * i] = 1;
                                }
                                else 
                                {
                                    QRCode[16 - j, 5 - k - 2 * i] = 0;
                                    if ((16 - j + 5 - k - 2 * i) % 2 == 0) //Masque
                                        QRCode[16 - j, 5 - k - 2 * i] = 1;
                                }
                            }
                        } //359 blocs

                //Le masque désormais (cases bleues)
                for (int i = 0; i < 7; i++) QRCode[24 - i, 8] = mask[i];
                for (int i = 0; i < 8; i++) QRCode[8, 17 + i] = mask[7 + i];

                for (int i = 0; i < 6; i++) QRCode[8, i] = mask[i];
                for (int i = 0; i < 2; i++) QRCode[8, 7 + i] = mask[6 + i];
                for (int i = 0; i < 7; i++) QRCode[6 - i, 8] = mask[8 + i];
                //Fin de matrice version 2

                //Application du masque
            
            }

            QRCode = Aggrandir2(QRCode);

            //Initialisation de l'image finale
            Pixel[,] QRFinal = new Pixel[QRCode.GetLength(0), QRCode.GetLength(1)];
            for (int i = 0; i < QRFinal.GetLength(0); i++)
                for (int j = 0; j < QRFinal.GetLength(1); j++)
                    QRFinal[i, j] = new Pixel();

            //On met des cases à la bonne couleur
            for (int i = 0; i < QRFinal.GetLength(0); i++)
                for (int j = 0; j < QRFinal.GetLength(1); j++)
                {
                    if(QRCode[i,j] == 0)
                    {
                        QRFinal[i, j].Bleu = 255;
                        QRFinal[i, j].Vert = 255;
                        QRFinal[i, j].Rouge = 255;
                    }
                    else
                    {
                        QRFinal[i, j].Bleu = 0;
                        QRFinal[i, j].Vert = 0;
                        QRFinal[i, j].Rouge = 0;
                    }
                }

            image = QRFinal;
            hauteur = QRFinal.GetLength(0);
            largeur = QRFinal.GetLength(1);
            size = largeur * hauteur;

            MiroirHautBas(); //Miroir car le QR Code était dans le mauvais sens

            GenererQRCode(); //On aurait sûrement pu utiliser from image to files mais j'ai préféré recréer une méthode spéciale pour être sûr
        }
        /// <summary>
        /// Convertit un caractère alphanumérique en un nombre selon la table alphanumérique
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public int CharEnAlphaNum(char alpha) //je n'ai pas trouvé de fonction qui le fait pour moi j'en ai donc créée une
        {
            int ret = 37; //Si je ne sais pas quel caractère c'est, on aura par défaut un $

            //On transforme le caractère en le numéro correspondant
            if (alpha == '0') ret = 0;
            if (alpha == '1') ret = 1;
            if (alpha == '2') ret = 2;
            if (alpha == '3') ret = 3;
            if (alpha == '4') ret = 4;
            if (alpha == '5') ret = 5;
            if (alpha == '6') ret = 6;
            if (alpha == '7') ret = 7;
            if (alpha == '8') ret = 8;
            if (alpha == '9') ret = 9;
            if (alpha == 'A') ret = 10;
            if (alpha == 'B') ret = 11;
            if (alpha == 'C') ret = 12;
            if (alpha == 'D') ret = 13;
            if (alpha == 'E') ret = 14;
            if (alpha == 'F') ret = 15;
            if (alpha == 'G') ret = 16;
            if (alpha == 'H') ret = 17;
            if (alpha == 'I') ret = 18;
            if (alpha == 'J') ret = 19;
            if (alpha == 'K') ret = 20;
            if (alpha == 'L') ret = 21;
            if (alpha == 'M') ret = 22;
            if (alpha == 'N') ret = 23;
            if (alpha == 'O') ret = 24;
            if (alpha == 'P') ret = 25;
            if (alpha == 'Q') ret = 26;
            if (alpha == 'R') ret = 27;
            if (alpha == 'S') ret = 28;
            if (alpha == 'T') ret = 29;
            if (alpha == 'U') ret = 30;
            if (alpha == 'V') ret = 31;
            if (alpha == 'W') ret = 32;
            if (alpha == 'X') ret = 33;
            if (alpha == 'Y') ret = 34;
            if (alpha == 'Z') ret = 35;
            if (alpha == ' ') ret = 36;
            if (alpha == '$') ret = 37;
            if (alpha == '%') ret = 38;
            if (alpha == '*') ret = 39;
            if (alpha == '+') ret = 40;
            if (alpha == '-') ret = 41;
            if (alpha == '.') ret = 42;
            if (alpha == '/') ret = 43;
            if (alpha == ':') ret = 44;

            return ret;
        }
        /// <summary>
        /// Convertis la valeur en /taille/ de bits
        /// </summary>
        /// <param name="val"></param>
        /// <param name="taille"></param>
        /// <returns>int[] de taille "taille" qui correspond à la valeur en bits de "val"</returns>
        public static int[] ConvertEnBits(int val, int taille)
        {
            //Déclaration des variables
            int[] ret = new int[taille];
            double valeur = val;
            int puissance = 0;

            //puissance devient le nombre exact de bits dont on a besoin pour écrire le nombre
            do
            {
                valeur = valeur / 2;
                puissance++;
            } while (valeur >= 1);

            int[] binaire = new int[puissance]; //On crée un nouveau tableau pour acceuillir le nombre

            for (int i = 0; i < binaire.Length; i++) binaire[i] = 0; //On initialise les bits à 0
            valeur = val; //On remet la valeur à la bonne val de départ

            //On met binaire au nombre en question
            for (int i = 0; i < puissance; i++)
                if (valeur >= Math.Pow(2, puissance - i - 1))
                {
                    valeur -= Math.Pow(2, puissance - i - 1);
                    binaire[i] = 1;
                }

            puissance = taille - puissance; //Puissance devient la différence entre taille et puissance
            for (int i = 0; i < puissance; i++) ret[i] = 0; //On remplit les cases superficielles de 0
            for (int i = puissance; i < taille; i++) ret[i] = binaire[i - puissance]; //On remplit le reste avec le nombre binaire
            return ret;
        }
        /// <summary>
        /// Change un tableau de 8 int, 0 ou 1 en un octet
        /// </summary>
        /// <param name="tab"></param>
        /// <returns>un octet de la valeur des 8 bits enregistrés en int</returns>
        public byte ConvertirEnByte(int[] tab)
        {
            int val = 0;
            for (int i = 0; i < 8; i++) val += tab[i] * Convert.ToInt32(Math.Pow(2, 7 - i));
            return Convert.ToByte(val);
        }

        public int[,] CreerQRCode(bool version)
        {
            if (version)
            {
                int[,] ret = new int[21, 21] {{0,0,0,0,0,0,0,255,0,0,0,0,0,255,0,0,0,0,0,0,0 },
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,255,0,255,255,255,255,255,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,255,0,255,0,0,0,255,0},
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,255,0,255,0,0,0,255,0},
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,255,0,255,0,0,0,255,0},
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,255,0,255,255,255,255,255,0 },
                                          { 0,0,0,0,0,0,0,255,0,255,0,255,0,255,0,0,0,0,0,0,0},
                                          { 255,255,255,255,255,255,255,255,0,0,0,0,0,255,255,255,255,255,255,255,255},
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                          { 0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 255,255,255,255,255,255,255,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0} };
                for(int i = 0; i < 21; i++) //Transforme ça en 1 et en 0 pour me simplifier la vie
                    for(int j = 0; j < 21; j++)
                    {
                        if (ret[i, j] == 0) ret[i, j] = 1;
                        else { ret[i, j] = 0; }
                    }
                return ret;
            }
            else
            {
                int[,] ret = new int[25, 25] {{0,0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,255,0,0,0,0,0,0,0 },
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,0,0,0,0,255,0,255,255,255,255,255,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,255,0,255,0,0,0,255,0},
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,255,0,255,0,0,0,255,0},
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,255,0,255,0,0,0,255,0},
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,0,0,0,0,255,0,255,255,255,255,255,0 },
                                          { 0,0,0,0,0,0,0,255,0,255,0,255,0,255,0,255,0,255,0,0,0,0,0,0,0},
                                          { 255,255,255,255,255,255,255,255,0,0,0,0,0,0,0,0,0,255,255,255,255,255,255,255,255},
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                          { 0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 255,255,255,255,255,255,255,255,0,0,0,0,0,0,0,0,0,255,255,255,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,255,0,255,0,0,0,0,0 },
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,0,0,0,0,255,255,255,0,0,0,0,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,0,0,0,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,255,255,255,255,255,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                                          { 0,0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0} };
                for (int i = 0; i < 25; i++) //Transforme ça en 1 et en 0 pour me simplifier la vie
                    for (int j = 0; j < 25; j++)
                    {
                        if (ret[i, j] == 0) ret[i, j] = 1;
                        else { ret[i, j] = 0; }
                    }
                return ret;
            }
        }
        /// <summary>
        /// Méthode qui part du QRCode pour le transformer en fichier
        /// </summary>
        public void GenererQRCode()
        {
            byte[] entete = NouvelEntete(image);
            byte[] nouvelleimage = new byte[54 + image.GetLength(0) * image.GetLength(1) * 3];

            for (int i = 0; i < 54; i++) nouvelleimage[i] = entete[i];

            //Remplissage du reste des données à partir de notre image actuelle
            int count = 0;

            for (int i = 0; i < hauteur; i++) //Remplissage de la matrice
            {
                for (int j = 0; j < largeur; j++)
                {
                    nouvelleimage[54 + count] = image[i, j].Bleu;
                    nouvelleimage[54 + 1 + count] = image[i, j].Vert;
                    nouvelleimage[54 + 2 + count] = image[i, j].Rouge;
                    count += 3;
                }
            }

            //Enregistrement et lecture du QRCode
            File.WriteAllBytes("QRCode.bmp", nouvelleimage); //Enregistrement de l'image dans un fichier appelé QRCode.bmp
            Process p = new();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = "QRCode.bmp";
            p.Start(); //Ouverture du QRCode
        }
        /// <summary>
        /// Affiche la matrice de int en paramètre
        /// </summary>
        /// <param name="matrice"></param>
        public void AfficherMatrice(int[,] matrice)
        {
            for(int i = 0; i < matrice.GetLength(0); i++)
            {
                Console.WriteLine();
                for (int j = 0; j < matrice.GetLength(1); j++)
                    Console.Write(matrice[i, j] + " ");
            }
        }
        /// <summary>
        /// Méthode pour augmenter la taille du QRCode
        /// </summary>
        /// <param name="matrice"></param>
        /// <returns></returns>
        public int[,] Aggrandir2(int[,] matrice)
        {
            int[,] ret = new int[4 * matrice.GetLength(0), 4 * matrice.GetLength(1)];
            for (int i = 0; i < ret.GetLength(0); i++)
                for (int j = 0; j < ret.GetLength(1); j++)
                    ret[i, j] = matrice[i / 4, j / 4]; 
            return ret;
        }
        /// <summary>
        /// Miroir car le QRCode sortait en inversé
        /// </summary>
        public void MiroirHautBas()
        {
            Pixel[,] NewImage = new Pixel[hauteur, largeur];
            for(int i = 0; i < hauteur; i++) 
                for(int j = 0; j < largeur; j++)
                {
                    NewImage[i, j] = new Pixel();
                    NewImage[i, j] = image[hauteur - 1 - i, j];
                }
            image = NewImage;
        }
        /// <summary>
        /// Transforme l'image de base en image cmposée de noir, de blanc, de rouge, de vert, de bleu, de jaune, de cyan et de violet (rose preque) uniquement
        /// </summary>
        public void innovation()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {

                    if (image[i, j].Bleu < 128) image[i, j].Bleu = 0; //Affiche le bleu au max si il est déjà présent à plus de 127 sinon on le met à 0
                    else { image[i, j].Bleu = 255; }
                    if (image[i, j].Vert < 128) image[i, j].Vert = 0; //Affiche le vert au max si il est déjà présent à plus de 127 sinon on le met à 0
                    else { image[i, j].Vert = 255; }
                    if (image[i, j].Rouge < 128) image[i, j].Rouge = 0; //Affiche le rouge au max si il est déjà présent à plus de 127 sinon on le met à 0
                    else { image[i, j].Rouge = 255; }
                }
            }
            From_Image_To_File();
        }
        /// <summary>
        /// Inverse les couleurs de l'image donnée (blanc devient noir et ainsi de suite)
        /// </summary>
        public void InverserLesCouleurs()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j].Bleu = Convert.ToByte(255 - image[i, j].Bleu);
                    image[i, j].Vert = Convert.ToByte(255 - image[i, j].Vert);
                    image[i, j].Rouge = Convert.ToByte(255 - image[i, j].Rouge);
                }
            }
            From_Image_To_File();
        }
    }
}