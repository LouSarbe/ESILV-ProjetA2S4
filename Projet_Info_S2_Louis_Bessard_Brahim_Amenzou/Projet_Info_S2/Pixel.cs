using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Info_S2
{
    class Pixel
    {
        //Déclaration
        byte[] tab = new byte[3];

        //Propriétés

        public byte[] Octet  //Initialise et retourne la valeur de l'octet correspondant
        {
            get { return tab; }
            set { tab = value; }
        }

        public byte Bleu  //Initialise et retourne le bit de la couleur bleu du pixel
        {
            get { return tab[0]; }
            set { tab[0] = value; }
        }
        public byte Vert //Initialise et retourne le bit de la couleur vert du pixel
        {
            get { return tab[1]; }
            set { tab[1] = value; }
        }
        public byte Rouge //Initialise et retourne le bit de la couleur rouge du pixel
        {
            get { return tab[2]; }
            set { tab[2] = value; }
        }
        
        

        //Constructeur
        public Pixel()
        {

        }

    }
}
