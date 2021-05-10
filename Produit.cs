using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cookapp
{
    class Produit
    {
        public string NomProduit{ get; set; }
        public float Quantite { get; set; }
        public int Occurrence { get; set; }
        public float StockMin { get; set; }
        public float StockMax { get; set; }
        public float StockActuel { get; set; }
        public float Qcommande { get; set; }
        public Fournisseur Fournit { get; set; }

    }
}
