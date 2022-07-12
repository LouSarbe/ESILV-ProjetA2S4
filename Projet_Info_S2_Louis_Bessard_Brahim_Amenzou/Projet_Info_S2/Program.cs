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

namespace Projet_Info_S2
{
    class Program
    {
        static void Main(string[] args)
        {
            MyImage image = new MyImage("coco.bmp");
            MyImage image2 = new MyImage("coco2.bmp");
            MyImage image3 = new MyImage("coco3.bmp");
            ConsoleKeyInfo cki;

            //Programme pour choisir les différentes méthodes
            do
            {
                Console.Clear();
                Console.WriteLine("Actions : \n" +
                   "Action 1 : Afficher l'image de base \n" +
                   "Action 2 : Nuances de gris \n" +
                   "Action 3 : Noir et blanc \n" +
                   "Action 4 : Rotation \n" +
                   "Action 5 : Miroir \n" +
                   "Action 6 : Détection de contours \n" +
                   "Action 7 : Renforcement des bords \n" +
                   "Action 8 : Flou \n" +
                   "Action 9 : Repoussage des bords \n" +
                   "Action 10 : Aggrandir l'image \n" +
                   "Action 11 : Rétrécir l'image \n" +
                   "Action 12 : Dessiner une fractale \n" +
                   "Action 13 : Données de l'histogramme \n" +
                   "Action 14 : Histogrammes des 3 couleurs de l'image \n" +
                   "Action 15 : Coder une image dans une autre \n" +
                   "Action 16 : Décoder une image qui est cachée en une autre : affiche d'abord l'image évidente et ensuite l'image décodée\n" +
                   "Action 17 : Générer un QR Code\n" +
                   "Action 18 : Innovation, couleurs primaires\n" +
                   "Action 19 : Innovation 2 : Inverser les couleurs");
                int action = SaisieNombre();
                Console.Clear();
                switch (action)
                {
                    case 1:
                        Console.WriteLine("Vous avez sélectionné Afficher l'image de base" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.From_Image_To_File();
                        break;
                    case 2:
                        Console.WriteLine("Vous avez sélectionné Nuances de gris" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageNuanceGris();
                        image.From_Image_To_File();
                        break;
                    case 3:
                        Console.WriteLine("Vous avez sélectionné Noir et blanc" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageNoirBlanc();
                        image.From_Image_To_File();
                        break;
                    case 4:
                        Console.WriteLine("Vous avez sélectionné Rotation angle quelconque" + "\n\n");
                        Console.Write("De quel angle voulez-vous faire tourner l'image ? (vous pouvez seulement tourner de 90°, 180° ou 270°) ");
                        int angle = Convert.ToInt32(Console.ReadLine());
                        image.TournerImage(angle);
                        image.From_Image_To_File();
                        break;
                    case 5:
                        Console.WriteLine("Vous avez sélectionné Effet miroir" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageMirroir();
                        image.From_Image_To_File();
                        break;
                    case 6:
                        Console.WriteLine("Vous avez sélectionné Detection de contours" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageDetectionContour();
                        image.From_Image_To_File();
                        break;
                    case 7:
                        Console.WriteLine("Vous avez sélectionné Renforcement des bords" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageBordsRenforces();
                        image.From_Image_To_File();
                        break;
                    case 8:
                        Console.WriteLine("Vous avez sélectionné Flou" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageFloue();
                        image.From_Image_To_File();
                        break;
                    case 9:
                        Console.WriteLine("Vous avez sélectionné Repoussage" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.ImageRepoussage();
                        image.From_Image_To_File();
                        break;
                    case 10:
                        Console.WriteLine("Vous avez selectionné Aggrandir l'image" + "\n\n");
                        Console.WriteLine("De quelle valeur voulez vous aggrandir l'image ?");
                        int valeur = Convert.ToInt32(Console.ReadLine());
                        Console.ReadKey();
                        image.AggrandirImage(valeur);
                        image.From_Image_To_File();
                        break;
                    case 11:
                        Console.WriteLine("Vous avez selectionné Rétrécir l'image" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        //Console.WriteLine("De quelle valeur voulez vous rétrécir l'image ?");
                        //int valeur2 = Convert.ToInt32(Console.ReadLine());
                        Console.ReadKey();
                        image.RetrecirImage();
                        image.From_Image_To_File();
                        break;
                    case 12:
                        Console.WriteLine("Vous avez selectionné Dessiner une fractale" + "\n\n");
                        Console.WriteLine("A quel niveau de précision voulez vous aller ? (éviter de mettre un nombre trop important) ");
                        int val = Convert.ToInt32(Console.ReadLine());
                        Console.ReadKey();
                        int[,] fractale = new int[1024, 1024];
                        image.Fractale(val, fractale);
                        Process p = new();
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.FileName = "ImageFractale.bmp";
                        p.Start(); //Ouverture de l'image
                        Console.ReadLine();
                        break;
                    case 13:
                        Console.WriteLine("Vous avez selectionné Données de l'histogramme" + "\n\n");
                        image.HistogrammeDonnees();
                        break;
                    case 14:
                        Console.WriteLine("Vous avez selectionné Histogrammes des 3 couleurs" + "\n\n");
                        image.HistogrammeImage();
                        Console.ReadKey();
                        Process p2 = new();
                        p2.StartInfo.UseShellExecute = true;
                        p2.StartInfo.FileName = "HistoBleu.bmp";
                        p2.Start(); //Ouverture de l'image
                        Console.ReadLine();
                        p2.StartInfo.UseShellExecute = true;
                        p2.StartInfo.FileName = "HistoVert.bmp";
                        p2.Start(); //Ouverture de l'image
                        Console.ReadLine();
                        p2.StartInfo.UseShellExecute = true;
                        p2.StartInfo.FileName = "HistoRouge.bmp";
                        p2.Start(); //Ouverture de l'image
                        Console.ReadLine();
                        break;
                    case 15:
                        Console.WriteLine("Vous avez selectionné Coder une image dans une autre" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.CoderImage(image2.Image);
                        image.From_Image_To_File();
                        break;
                    case 16:
                        Console.WriteLine("Vous avez selectionné Décoder une image" + "\n\n");
                        image3.DecoderImage();
                        Console.ReadKey();
                        Process p3 = new();
                        p3.StartInfo.UseShellExecute = true;
                        p3.StartInfo.FileName = "ImageEvidente.bmp";
                        p3.Start(); //Ouverture de l'image
                        Console.ReadLine();
                        p3.StartInfo.UseShellExecute = true;
                        p3.StartInfo.FileName = "ImageCachee.bmp";
                        p3.Start(); //Ouverture de l'image
                        Console.ReadLine();
                        break;
                    case 17:
                        Console.WriteLine("Vous avez selectionné Générer un QR Code" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.QRCode();
                        break;
                    case 18:
                        Console.WriteLine("Vous avez selectionné Innovation : couleurs primaires" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.innovation();
                        break;
                    case 19:
                        Console.WriteLine("Vous avez selectionné Innovation 2 : inverser les couleurs" + "\n\n");
                        Console.WriteLine("Appuyez sur Entrer");
                        Console.ReadKey();
                        image.InverserLesCouleurs();
                        break;
                }
                Console.WriteLine();
                Console.WriteLine("Tapez Escape pour sortir ou une autre touche pour continuer");
                cki = Console.ReadKey();
                Console.WriteLine();
            } while (cki.Key != ConsoleKey.Escape);
            Console.ReadKey();
        }
        public static int SaisieNombre() //Sélection de l'exercice
        {
            int resultat = 0;
            do
                Console.Write("Entrez le numéro correspondant à l'action que vous voulez réaliser : ");
            while (!int.TryParse(Console.ReadLine(), out resultat));
            return resultat;
        }
    }
}
