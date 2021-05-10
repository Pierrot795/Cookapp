using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Console;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;

namespace cookapp
{   public enum TypeProduit
    {
        viande,
        poisson,
        oeuf,  
        laitage, 
        fruit, 
        légume, 
        condiment,
        féculent, 
        huile,
        alcool,
        autre
    }
    public enum Unite
    {
        g,
        cueillere_a_soupe,
        cL,
        unite
    }
    class BaseDeDonnées 
    {
        #region champs
        #endregion

        #region propriétés
        /// <summary>
        /// chaine de connection permettant de créer une instance de connexion et de lier C# à la base
        /// </summary>
        private string ConnexionString { get { return "SERVER=localhost;PORT=3306;DATABASE=cook;UID=root;PASSWORD=MDP_MYSQL;"; } }
        #endregion

        #region Constructeur
        #endregion

        #region méthodes
        /// <summary>
        /// Crée l'instance de connexion l'ouvre et la retourne pour qu'on puisse s'en servir pour créer des commandes
        /// </summary>
        /// <returns>retourne la connection</returns>
        public MySqlConnection connexion()
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(ConnexionString);
                connection.Open();
            }
            catch (MySqlException e)
            {
                WriteLine("ErreurConnexion: " + e.ToString());
            }
            return connection;

        }

        /// <summary>
        /// NonQuery(MySqlCommand command) execute une requete qui ne nécessite pas de sélection.
        /// </summary>
        /// <param name="command">prend en paramètre une commande qui ne nécessite pas de sélection</param>
        public static void NonQuery(MySqlCommand command)
        {
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                WriteLine("ErreurConnexion" + e.ToString());
                ReadLine();
            }
            command.Dispose();
        }

        /// <summary>
        /// Deconnexion(MySqlConnection connection) ferme la connexion à la base de données, quand un utilisateur se déconnecte.
        /// </summary>
        /// <param name="connection">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void Deconnexion(MySqlConnection connection)
        {
            connection.Close(); 
        }

        /// <summary>
        /// VerifExistenceInstance() prend une valeur et regarde si elle est présente dans une table comme égale à un attribut ou non.
        /// Si c'est le cas elle renvoie True,False sinon.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <param name="nomInstance">Il s'agit de la valeur dont on veut tester la présence</param>
        /// <param name="attribut">Il s'agit de l'attribut pour lequel on veut tester la valeur</param>
        /// <param name="table">Il s'agit de la table à laquelle appartient l'attribut pour lequel on veut tester la valeur</param>
        /// <returns>retourne vrai si la valeur est dans la table, et false sinon</returns>
        public static bool VerifExistenceInstance(MySqlConnection connexion, string nomInstance, string attribut, string table)
        {
            bool existence = false;
            MySqlCommand verification = connexion.CreateCommand();
            verification.Parameters.Add(new MySqlParameter("@instance", MySqlDbType.VarChar, value: nomInstance, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            verification.CommandText = "SELECT " + attribut + " FROM cook." + table + " WHERE " + attribut + " = @instance;";
            MySqlDataReader reader = verification.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetString(i) == nomInstance.ToString())
                    {
                        existence = true;
                    }
                }
            }
            reader.Close();
            verification.Dispose();
            return existence;
        }

        /// <summary>
        /// AfficherCdrs(MySqlConnection connexion) affiche la liste des créateurs de recettes et les classes selon leurs nombres totaux
        /// de commandes dans l'ordre décroissant.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void AfficherCdrs(MySqlConnection connexion)
        {
            WriteLine("Le nombre de créateurs de recette de notre site est actuellement de " + CompterTuplesTable(connexion, "cook.cdr"));
            WriteLine();
            WriteLine("Voici leur liste: ");
            WriteLine();
            MySqlCommand command = connexion.CreateCommand();
            command.CommandText = "SELECT r.codeCreateur,c.nomClient,sum(k.occurrencesRecette) from cook.compositionCommande k,cook.client c,cook.commande co,cook.recette r WHERE co.numCommande = k.numCommande and k.nomRecette = r.nomRecette and c.codeClient =r.codeCreateur group by r.codeCreateur order by sum(k.occurrencesRecette) desc ;";
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                WriteLine("Le createur " + reader.GetString(1) + " (" + reader.GetString(0) + ") a été commandé " + reader.GetInt32(2) + " fois.");
            }
            reader.Close();
            command.Dispose();
        }

        /// <summary>
        /// CompterTuplesTable(MySqlConnection connexion,string table) renvoie le nombre de tuples d'une table
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <param name="table">il s'agit de la table dont on veut compter le nombre de tuples</param>
        /// <returns>retourne le nombre de tuples de la table en question</returns>
        public static int CompterTuplesTable(MySqlConnection connexion,string table)
        {
            MySqlCommand comptage = connexion.CreateCommand();
            comptage.CommandText = "Select count(*) FROM " + table;
            MySqlDataReader reader = comptage.ExecuteReader();
            int compteur = 0;
            while (reader.Read())
            {
                compteur = reader.GetInt32(0);
            }
            reader.Close();
            comptage.Dispose();
            return compteur;

        }

        /// <summary>
        /// AfficherTuples(MySqlConnection connexion,string table,string condition = null) affiche les tuples d'une table
        /// avec certaines conditions (WHERE) ou non.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <param name="table">il s'agit de la table dont on veut afficher les tuples</param>
        /// <param name="condition">condition restreignant la quantité de tuples à afficher (précision)</param>
        public static void AfficherTuples(MySqlConnection connexion,string table,string condition = null)
        {
            MySqlCommand affichage = connexion.CreateCommand();
            affichage.CommandText = "SELECT * FROM cook." + table + condition;
            MySqlDataReader reader = affichage.ExecuteReader();
            while (reader.Read())
            {
                string currentrow = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    currentrow += reader.GetString(i) + ' ';
                }
                WriteLine(currentrow);
            }
            reader.Close();
            affichage.Dispose();
        }

        /// <summary>
        ///  QProduitCommandes() crée un dictictionnaire prenant pour clé le nom d'un produit, et pour valeur le stock actualisé à un moment donné
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <param name="date">date de livraison</param>
        /// <param name="stock">stock à décrémenter après commandes</param>
        /// <param name="complement"></param>
        /// <param name="cookparam">contrainte sur le cuisinier qui a cuisiné les commandes</param>
        /// <param name="complement2"></param>
        /// <param name="livree">il s'agit en général des commandes deja livrées</param>
        /// <returns>retourne les quantites à commander</returns>
        public static Dictionary<string, float> QProduitCommandes(MySqlConnection connexion, DateTime date, string stock, string complement = null, string cookparam = null,string complement2 = null,string livree = null) 
        {
            Dictionary<string, float> quantites = new Dictionary<string, float>();

            MySqlCommand commandes = connexion.CreateCommand();
            commandes.Parameters.Add(new MySqlParameter("@idcook", MySqlDbType.VarChar, value: cookparam, size:6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandes.Parameters.Add(new MySqlParameter("@livree", MySqlDbType.Enum, value: livree, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandes.Parameters.Add(new MySqlParameter("@date", MySqlDbType.DateTime, value: date, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandes.CommandText = "SELECT cr.nomProd,p."+stock+"- sum(cc.occurrencesRecette*cr.quantiteProduit) FROM cook.compositionCommande cc,produit p,cook.contenanceRecette cr,cook.commande c  WHERE cc.nomRecette = cr.recetteNom AND c.numCommande = cc.numCommande and p.nomProduit = cr.nomProd  and c.dateLivraison = @date " + complement+complement2+ " group by cr.nomProd;"; 
            MySqlDataReader reader = commandes.ExecuteReader();
            while (reader.Read()) 
            {
                quantites.Add(reader.GetString(0), reader.GetFloat(1));
            }

            reader.Close();
            commandes.Dispose();

            MySqlCommand ajouts = connexion.CreateCommand();
            ajouts.CommandText = "SELECT nomProduit," + stock + " FROM cook.produit;";
            reader = ajouts.ExecuteReader();
            while (reader.Read())
            {
                if(quantites.ContainsKey(reader.GetString(0)) == false)
                {
                    quantites.Add(reader.GetString(0), reader.GetFloat(1));
                }
            }
            reader.Close();
            ajouts.Dispose();

            return quantites;
        }

        /// <summary>
        /// BasseQuantite() retourne la liste des produits de quantités inférieures à 1 au 2 fois le stock minimal requis
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <param name="option">condition éventuelle, par exemple si on veut le double du stockMin pour le comparer au stock actuel</param>
        /// <returns>retourne la liste des produits de quantités inférieures à 1 au 2 fois le stock minimal requis</returns>
        public static List<Produit> BasseQuantite(MySqlConnection connexion,string option = null)
        {
            List<Produit> produits = new List<Produit>();
            MySqlCommand command = connexion.CreateCommand();
            command.CommandText = "SELECT nomProduit,stockActuel,stockMin - stockActuel ,stockMax FROM cook.produit WHERE stockActuel <= "+option+"stockMin";
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if(option == "2*")
                {
                    WriteLine("Le produit " + reader.GetString(0) + " a un stock inférieur au double de son stock minimal");
                }
                produits.Add(new Produit { NomProduit = reader.GetString(0),StockActuel = reader.GetFloat(1) ,Qcommande = reader.GetFloat(2),StockMax = reader.GetFloat(3) });
            }
            reader.Close();
            command.Dispose();
            return produits;
        }

        public static void RecettesBasseQuantite(MySqlConnection connexion)
        {
            List<Produit> produits = BasseQuantite(connexion,"2*");

            WriteLine();

            bool faux = true;
            string entree = null;

            while(faux == true)
            {
                WriteLine("Entrez le nom du produit que vous souhaitez inspecter.");
                entree = ReadLine();
                foreach (Produit p in produits)
                {
                    if (p.NomProduit == entree)
                    {
                        faux = false;
                    }
                }

            }
            
            MySqlCommand recette = connexion.CreateCommand();
            recette.Parameters.Add(new MySqlParameter("@produit", MySqlDbType.VarChar, value: entree, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            recette.CommandText = "SELECT cr.recetteNom,cr.QuantiteProduit,p.unité FROM cook.contenanceRecette cr,cook.produit p  WHERE p.nomProduit = cr.nomProd and cr.nomProd =  @produit;";
            MySqlDataReader reader = recette.ExecuteReader();
            while (reader.Read())
            {
                WriteLine("Ce produit est utilisé à raison de " + reader.GetFloat(1) + " " + reader.GetString(2) + " pour la confection de : " + reader.GetString(0));
            }
            reader.Close();
            recette.Dispose();
        }
        #endregion
    } 
}
