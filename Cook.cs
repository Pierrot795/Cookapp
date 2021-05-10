using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using static System.Console;
using System.Xml;

namespace cookapp
{
    class Cook
    {
        #region champs
        private static List<Produit> produitsManquants = new List<Produit>();
        #endregion

        #region constructeur
        public Cook(string idCook,string mdpCook)
        {
            this.IdCook = idCook;
            this.MdpCook = mdpCook;
        }
        #endregion

        #region propriétés

        /// <summary>
        /// Identifiant du cuisinier
        /// </summary>
        public string IdCook { get; }

        /// <summary>
        /// mot de passe du cuisinier
        /// </summary>
        private string MdpCook { get; set; }

        #endregion

        #region méthodes
        /// <summary>
        /// Cooker Login(MySqlConnection connexion) permet à un cuisinier de se connecter, en entrant son mot de passe et son identifiant.
        /// Si les deux valeurs ne sont pas dans la base il doit retaper les bonnes, sinon un objet Cooker avec ses identifiants est créé.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <returns>retourne le cuisinier pour lui permettre d'effectuer des actions</returns>
        public static Cook Login(MySqlConnection connexion)
        {
            bool connecte = false;
            string code = null;
            string mdp = null;
            while (connecte == false)
            {
                WriteLine("Veuillez entrer votre ID.");
                code = ReadLine();
                WriteLine("Veuillez entrer votre mot de passe.");
                mdp = ReadLine();

                MySqlCommand log = connexion.CreateCommand();
                log.Parameters.Add(new MySqlParameter("@idcook", MySqlDbType.VarChar, value: code, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                log.Parameters.Add(new MySqlParameter("@motdepasse", MySqlDbType.VarChar, value: mdp, size: 24, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                log.CommandText = "SELECT idCook, mdpCook from cook.cooking WHERE idCook = @idcook AND mdpCook = @motdepasse;";
                MySqlDataReader reader = log.ExecuteReader();
                while (reader.Read())
                {
                    WriteLine(reader.GetString(0) + " " + reader.GetString(1));
                    if (reader.GetString(0) == code && reader.GetString(1) == mdp)
                    {
                        connecte = true;
                    }
                }
                reader.Close();
                log.Dispose();
            }
            return new Cook(code, mdp);
        }

        /// <summary>
        /// Cuisine(MySqlConnection connexion) crée tout d'abord un dictionnaire avec pour chaque produit le stock actualisé après que le
        /// cuisinier ait réalisé toutes ses commandes de la journée.
        /// Le stock est ainsi mis à jour pour chacun de ces produits dans la bdd.
        /// Chacune de ces commandes sont marquées comme livrées dans la cook.commande, et un fichier txt est créé pour enregistrer les
        /// cuisines/livraisons déjà effectuées par le cuisinier dans la journée, pour qu'il s'en rappelle au cas ou il
        /// aurait de nouvelles commandes à traiter avant la fin de la journée.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void Cuisine(MySqlConnection connexion) 
        {
            Dictionary<string, float> quantites = BaseDeDonnées.QProduitCommandes(connexion,DateTime.Today,"stockActuel","AND idCook =@idcook ",this.IdCook,"AND c.livree = @livree","non");


            foreach (KeyValuePair<string, float> entry in quantites)
            {

                MySqlCommand videStock = connexion.CreateCommand(); 
                videStock.Parameters.Add(new MySqlParameter("@produit", MySqlDbType.VarChar, value: entry.Key, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                videStock.Parameters.Add(new MySqlParameter("@qTotale", MySqlDbType.Int32, value: entry.Value, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                videStock.CommandText = "UPDATE cook.produit SET stockActuel = @qtotale WHERE nomProduit = @produit; ";
                BaseDeDonnées.NonQuery(videStock);
            }
            MySqlCommand commandecomplete = connexion.CreateCommand();
            commandecomplete.Parameters.Add(new MySqlParameter("@idcook", MySqlDbType.VarChar, value: this.IdCook, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandecomplete.CommandText = "UPDATE cook.commande SET livree = 'oui' where idCook = @idCook AND day(dateLivraison) = day(current_timestamp()) and  month(dateLivraison) = month(current_timestamp()) and livree = 'non';";
            BaseDeDonnées.NonQuery(commandecomplete);

            MySqlCommand ecrirefichier = connexion.CreateCommand();
            ecrirefichier.Parameters.Add(new MySqlParameter("@idcook", MySqlDbType.VarChar, value: this.IdCook, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            ecrirefichier.CommandText = "SELECT numCommande FROM cook.commande where idCook = @idCook AND day(dateLivraison) = day(current_timestamp()) and  month(dateLivraison) = month(current_timestamp()) and livree = 'oui';";
            MySqlDataReader reader = ecrirefichier.ExecuteReader();
            StreamWriter ecrire = new StreamWriter("commandes.txt", true);
            while (reader.Read())
            {

                ecrire.WriteLine("Commande n°" + reader.GetString(0) + " livrée le " + DateTime.Today);
            }
            ecrire.Close();
            reader.Close();
            commandecomplete.Dispose();

        }

        /// <summary>
        /// Commande(MySqlConnection connexion) crée tout d'abord une liste de produits dont le stock actuel est inférieur au stock minimal,
        /// avec pour quantité à commander pour défaut égale à stock minimal - stock actuel.
        /// Pour chaque produit commandé demain, on ajoute à la quantité à commander, la quantité nécessaire pour réaliser les différentes commandes,
        /// sum(cc.occurrencesRecette*cr.quantiteProduit),dans la limite du stock maximal.
        /// dans la limite du stock maximal.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        /// <returns>Liste de produits avec la quantité à commander (QCommande) actualisée.</returns>
        public static List<Produit> Commande(MySqlConnection connexion) 
        {
            List<Produit> Acommander = BaseDeDonnées.BasseQuantite(connexion); 
            MySqlCommand quantitesSemaine = connexion.CreateCommand();
            quantitesSemaine.Parameters.Add(new MySqlParameter("@demain", MySqlDbType.DateTime, value: DateTime.Today.AddDays(1), size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));

            quantitesSemaine.CommandText = "SELECT cr.nomProd,sum(cc.occurrencesRecette*cr.quantiteProduit) FROM cook.compositionCommande cc,cook.contenanceRecette cr,cook.commande c WHERE cc.numCommande = c.numCommande AND cc.nomRecette = cr.recetteNom AND c.dateLivraison = @demain group by cr.nomProd;";
            MySqlDataReader reader = quantitesSemaine.ExecuteReader();
            while (reader.Read()) 
            {
                for(int i = 0; i < Acommander.Count; i++)
                {
                    if(Acommander[i].NomProduit == reader.GetString(0)) 
                    {
                        Acommander[i].Qcommande += reader.GetFloat(1); 

                    }
                }
            }
            reader.Close();
            quantitesSemaine.Dispose();
            return Acommander;


        }

        /// <summary>
        /// ListeXML(MySqlConnection connexion) prend la liste des produits à commander pour demain, avec les quantités à commander.
        /// La méthode attribue ensuite un fournisseur à chaque produit.
        /// Un fichier xml est créé avec pour chaque fournisseur, des éléments enfants produits, contenant des informations sur la quantité
        /// à commander.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void ListeXML(MySqlConnection connexion)
        {
            List<Produit> commande = Commande(connexion);
            List<Fournisseur> fournit = new List<Fournisseur>();

            XmlDocument docXml = new XmlDocument();
            XmlDeclaration xmldecl = docXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement racine = docXml.CreateElement("Commande");
            docXml.AppendChild(racine);
            docXml.InsertBefore(xmldecl, racine);

            foreach (Produit prod in commande)
            {
                MySqlCommand command = connexion.CreateCommand();
                command.CommandText = "SELECT codeF,mdpF FROM cook.fournisseur ORDER BY RAND() LIMIT 1;";
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    prod.Fournit = new Fournisseur{CodeF = reader.GetString(0),MdpF = reader.GetString(1)};
                    if(fournit.Contains(prod.Fournit) == false)
                    {
                        fournit.Add(prod.Fournit);
                    }
                }
                reader.Close();
                command.Dispose();

                MySqlCommand command2 = connexion.CreateCommand();
                command2.Parameters.Add(new MySqlParameter("@codeF", MySqlDbType.VarChar, value: prod.Fournit.CodeF, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command2.Parameters.Add(new MySqlParameter("@nomProduit", MySqlDbType.VarChar, value: prod.NomProduit, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command2.Parameters.Add(new MySqlParameter("@dateF", MySqlDbType.DateTime, value: DateTime.Today.AddDays(1), size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command2.Parameters.Add(new MySqlParameter("@quantiteF", MySqlDbType.Float, value: prod.Qcommande, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));

                command2.CommandText = "INSERT INTO cook.fournit(codeF,nomProduit,dateF,quantiteF) VALUES (@codeF,@nomProduit,@dateF,@quantiteF);";
                BaseDeDonnées.NonQuery(command2);
            }

            foreach(Fournisseur f in fournit)
            {
                XmlElement fournisseur = docXml.CreateElement("fournisseur");
                racine.AppendChild(fournisseur);
                foreach(Produit p in commande)
                {
                    if(p.Fournit == f)
                    {
                        XmlElement produit = docXml.CreateElement("produit");
                        fournisseur.AppendChild(produit);
                        produit.InnerText = "Le produit" + p.NomProduit + "doit etre fournit à raison de " + p.Qcommande.ToString() + " pour demain";
                    }
                }
            }
            docXml.Save("Approvisionnement.xml");
        }
        #endregion
    }
}
