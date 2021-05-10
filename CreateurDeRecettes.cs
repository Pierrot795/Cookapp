using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using static System.Console;

namespace cookapp
{
    class CreateurDeRecettes:Client
    {
        #region champs
        #endregion

        #region propriétés

        #endregion

        #region Constructeur
        public CreateurDeRecettes(string codeCreateur,string password) : base(codeClient: codeCreateur,password:password) { }
        #endregion

        #region méthodes
        /// <summary>
        /// CreationRecette(MySqlConnection connexion) demande au cdr de choisir la recette  qu'il veut créer, tout d'abord en terme de 
        /// nom,type et descriptifs,prix de vente en vérifiant qu'elle n'existe pas déjà.
        /// La recette est insérée dans la table recette.
        /// Il doit ensuite saisir les produits un par un ainsi que les quantités requises et les stocks avant des les insérer dans la table
        /// (stocks si le produit n'est pas deja dans la table).
        /// On insère également les tuples correspondant dans cook.contenanceRecette et on actualise les stocks min et max par rapport 
        /// à la création de notre recette.
        ///</summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void CreationRecette(MySqlConnection connexion)
        {
            WriteLine("Bienvenue dans le gestionnaire de création de recettes !");
            WriteLine("Veuillez saisir le type de votre recette:");
            string typeRecette = ReadLine();
            WriteLine("Veuillez saisir le nom de votre recette:");
            string ideeRecette = ReadLine();
            bool existence = BaseDeDonnées.VerifExistenceInstance(connexion,ideeRecette,"nomRecette","Recette");
            while(existence == true)
            {
                WriteLine("Cette recette est déjà répertoriée dans l'application. Veuillez entrer une autre recette.");
                ideeRecette = ReadLine();
                existence = BaseDeDonnées.VerifExistenceInstance(connexion, ideeRecette,"nomRecette", "Recette");
            }


            WriteLine("Veuillez décrire votre recette:"); 
            string descriptif = ReadLine();
            while(descriptif.Length > 256)
            {
                WriteLine("Votre descriptif doit se limiter à 256 caractères. Vous en avez tapé " + descriptif.Length);
                descriptif = ReadLine();
            }
            WriteLine("Veuillez saisir un prix pour cette recette entre 10 et 40 cooks:");
            float prix = 0;
            bool mauvaisprix = true;
            while (mauvaisprix)
            {
                prix = Program.TryParseFloat();
                if(prix >= 10 && prix <= 40)
                {
                    mauvaisprix = false;
                }
            }           

            MySqlCommand saisieRecette = connexion.CreateCommand();
            saisieRecette.Parameters.Add(new MySqlParameter("@recette", MySqlDbType.VarChar, value: ideeRecette, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default)); 
            saisieRecette.Parameters.Add(new MySqlParameter("@type", MySqlDbType.VarChar, value: typeRecette, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            saisieRecette.Parameters.Add(new MySqlParameter("@description", MySqlDbType.VarChar, value: descriptif, size: 256, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            saisieRecette.Parameters.Add(new MySqlParameter("@codeCdr", MySqlDbType.VarChar, value: this.CodeClient, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            saisieRecette.Parameters.Add(new MySqlParameter("@prixvente", MySqlDbType.Float, value: prix, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            saisieRecette.Parameters.Add(new MySqlParameter("@remuneration", MySqlDbType.Float, value: 2.0, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            saisieRecette.CommandText = "INSERT INTO cook.recette (nomRecette,type,descriptif,codeCreateur,prix,remunCdr) VALUES (@recette,@type,@description,@codeCdr,@prixvente,@remuneration);";
            BaseDeDonnées.NonQuery(saisieRecette);

            bool entrerIngredients = true;
            while (entrerIngredients)
            {
                WriteLine("Veuillez saisir le nom d'un ingrédient: ");
                string produit = ReadLine(); //s'il est dans la base c'est bon, sinon l'admin l'ajoute
                if(BaseDeDonnées.VerifExistenceInstance(connexion, produit, "nomProduit", "produit") == false)
                {
                    Unite unite = Unite.g;
                    WriteLine("Ce produit n'est pas disponible pour le moment. Il sera commandé à nos fournisseurs sous peu."); //VOIR SI JE PEUX PAS L INSERER QUAND MEME A PARTIR DE CETTE METHODE
                    TypeProduit typeDeProduit = TypeDeProduit();
                    if(typeDeProduit == TypeProduit.alcool || typeDeProduit == TypeProduit.huile)
                    {
                        unite = Unite.cL;
                    }
                    if(typeDeProduit == TypeProduit.condiment)
                    {
                        unite = Unite.cueillere_a_soupe;
                    }

                    MySqlCommand newproduit = connexion.CreateCommand();
                    newproduit.Parameters.Add(new MySqlParameter("@produit", MySqlDbType.VarChar, value: produit, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    newproduit.Parameters.Add(new MySqlParameter("@type", MySqlDbType.Enum, value: typeDeProduit, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    newproduit.Parameters.Add(new MySqlParameter("@unité", MySqlDbType.Enum, value: unite, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    Clear();
                    WriteLine("Veuillez choisir un stock minimal pour ce produit");
                    float stockMin = Program.TryParseFloat();
                    Clear();
                    WriteLine("Veuillez choisir un stock maximal pour ce produit");
                    float stockMax = Program.TryParseFloat();
                    Clear();
                    while(stockMin <= 0 || stockMin >= stockMax)
                    {
                        WriteLine("Veuillez choisir un stock minimal pour ce produit");
                        stockMin = Program.TryParseFloat();
                        Clear();
                        WriteLine("Veuillez choisir un stock minimal pour ce produit");
                        stockMax = Program.TryParseFloat();
                        Clear();
                    }

                    newproduit.Parameters.Add(new MySqlParameter("@stockMin", MySqlDbType.Float, value: stockMin, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    newproduit.Parameters.Add(new MySqlParameter("@stockMax", MySqlDbType.Float, value: stockMax, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    newproduit.CommandText = "INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES (@produit,@type,@unité,0,@stockMin,@stockMax)";
                    BaseDeDonnées.NonQuery(newproduit);

                }
                else
                {
                    WriteLine("Veuillez saisir la quantité de ce produit nécessaire à l'élaboration de la recette"); //afficher l'unité dans ce writeline
                    float quantite = Program.TryParseFloat(); //faire en sorte que si un chiffre n'est pas entré il ne se passe rien
                    MySqlCommand contenance = connexion.CreateCommand();
                    contenance.Parameters.Add(new MySqlParameter("@recette", MySqlDbType.VarChar, value: ideeRecette, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    contenance.Parameters.Add(new MySqlParameter("@prod", MySqlDbType.VarChar, value: produit, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    contenance.Parameters.Add(new MySqlParameter("@quanti", MySqlDbType.Float, value: quantite, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    contenance.CommandText = "INSERT INTO cook.contenanceRecette (recetteNom,nomProd,quantiteProduit) VALUES (@recette,@prod,@quanti);";
                    BaseDeDonnées.NonQuery(contenance);

                    MySqlCommand stocks = connexion.CreateCommand();
                    stocks.Parameters.Add(new MySqlParameter("@quanti", MySqlDbType.Float, value: quantite, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    stocks.Parameters.Add(new MySqlParameter("@prod", MySqlDbType.VarChar, value: produit, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                    stocks.CommandText = "UPDATE cook.produit SET stockMin = StockMin/2 +3*@quanti, stockMax = stockMax+2*@quanti WHERE nomProduit = @prod;";
                    BaseDeDonnées.NonQuery(stocks);
                } 
                WriteLine("Voulez vous ajouter un autre ingrédient ? ");
                WriteLine("Si oui appuyez sur n'importe quelle touche\nSinon tapez [N]");
                if (ReadKey(true).Key == ConsoleKey.N)
                {
                    entrerIngredients = false;
                }
            }

        }

        /// <summary>
        /// Affiche le solde cook du cdr
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void SoldeCook(MySqlConnection connexion)
        {
            MySqlCommand soldecommand = connexion.CreateCommand();
            soldecommand.Parameters.Add(new MySqlParameter("@code", MySqlDbType.VarChar, value: this.CodeClient, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            soldecommand.CommandText = "SELECT soldeCook FROM cook.cdr WHERE codeCdr = @code;";
            MySqlDataReader reader = soldecommand.ExecuteReader();
            while (reader.Read())
            {
                WriteLine("Votre solde de Cooks est de " + reader.GetInt32(0) + " cooks");

            }
            reader.Close();
            soldecommand.Dispose();
        }

        /// <summary>
        /// Permet au cdr de choisir le type des produits qu'il utilise (pour les insérer dans la table cook.produit s'ils ne le sont pas deja)
        /// </summary>
        /// <returns>retourne le type du produit</returns>
        public TypeProduit TypeDeProduit()
        {
            TypeProduit type = TypeProduit.autre;
            WriteLine("Afin d'entrer ce produit dans notre base de données, veuillez sélectionner son type parmi les suivants:");
            WriteLine("viande   : [V]" + "\n" + "poisson  : [P]" + "\n" + "laitage  : [L]" + "\n" + "fruit  : [F]" + "\n" + "condiment  : [C]" + "\n" + "féculent  : [T]" + "\n" + "féculent  : [T]" + "\n" + "alcool  : [A]" + "\n" + "autre  : [O]");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.V:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.viande;

                        break;
                    case ConsoleKey.P:
                        Clear(); //
                        bonneentrée = true;

                        break;
                    case ConsoleKey.L:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.laitage;

                        break;
                    case ConsoleKey.F:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.fruit;

                        break;
                    case ConsoleKey.E:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.légume;

                        break;
                    case ConsoleKey.T:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.féculent;

                        break;
                    case ConsoleKey.A:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.alcool;

                        break;
                    case ConsoleKey.C:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.condiment;

                        break;
                    case ConsoleKey.O:
                        Clear(); //
                        bonneentrée = true;
                        type = TypeProduit.autre;

                        break;
                    default:
                        break;
                }
            }
            return type;
        }

        /// <summary>
        /// Affiche la liste des recettes du cdr en question et le nombre de leurs commandes respectives
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void ListeRecettes(MySqlConnection connexion)
        {
            MySqlCommand command = connexion.CreateCommand();
            command.Parameters.Add(new MySqlParameter("@codeCdr", MySqlDbType.VarChar, value: this.CodeClient, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command.CommandText = "SELECT r.nomRecette,sum(k.occurrencesRecette) from cook.compositionCommande k,cook.recette r WHERE k.nomRecette = r.nomRecette and r.codeCreateur = @codeCdr group by r.nomRecette;";
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                WriteLine("La recette "+reader.GetString(0) +" que vous avez créée a été commandée " + reader.GetInt32(1) +" fois au total.");
            }
        }
       

        #endregion
    }
}
