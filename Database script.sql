DROP DATABASE cook;
CREATE DATABASE cook;
USE cook;

CREATE TABLE cook.client 
( codeClient VARCHAR(4) NOT NULL,
 mdp VARCHAR(24) NOT NULL,numTelClient VARCHAR(10) NOT NULL,
 nomClient VARCHAR(20) NOT NULL,
adresseClient VARCHAR(50) NOT NULL, 
villeClient VARCHAR(20) NOT NULL,
 PRIMARY KEY (codeClient),
INDEX f_mdp1_idx(mdp) );


CREATE TABLE cook.cdr
(codeCdr VARCHAR(4) NOT NULL,
mdp VARCHAR(24),
soldeCook FLOAT NULL,
PRIMARY KEY(codeCdr),
CONSTRAINT codeCdr 
FOREIGN KEY(codeCdr)
REFERENCES cook.client(codeClient)
 ON DELETE CASCADE ON UPDATE NO ACTION,
 CONSTRAINT mdp 
 FOREIGN KEY(mdp)
REFERENCES cook.client(mdp)
 ON DELETE CASCADE ON UPDATE NO ACTION);
 
 CREATE TABLE cook.cooking
 (idCook VARCHAR(5) NOT NULL,
 mdpCook VARCHAR(24) NOT NULL,
 nomCook VARCHAR(20) NOT NULL,
 prenomCook VARCHAR(20) NOT NULL,
numTelCook VARCHAR(10) NOT NULL,
 PRIMARY KEY(idCook) );


CREATE TABLE cook.commande
(numCommande VARCHAR(6) NOT NULL,
codeClient VARCHAR(4) NOT NULL,dateCommande DATETIME NOT NULL,
 dateLivraison DATETIME NOT NULL,
 idCook VARCHAR(5) NOT NULL,
 livree ENUM('oui','non'),
 PRIMARY KEY(numCommande),
CONSTRAINT codeClient
 FOREIGN KEY(codeClient)
 REFERENCES cook.client(codeClient) 
ON DELETE CASCADE ON UPDATE NO ACTION,
CONSTRAINT idCook
 FOREIGN KEY(idCook)
 REFERENCES cook.cooking(idCook) 
ON DELETE CASCADE ON UPDATE NO ACTION );


CREATE TABLE cook.recette
(nomRecette VARCHAR(20) NOT NULL,
type VARCHAR(50) NOT NULL,
descriptif VARCHAR(256) NOT NULL,
codeCreateur VARCHAR(4) NOT NULL,
prix FLOAT NULL,
remunCdr FLOAT NULL,
PRIMARY KEY(nomRecette),
CONSTRAINT codeCreateur
 FOREIGN KEY(codeCreateur)
 REFERENCES cook.cdr(codeCdr)
ON DELETE CASCADE ON UPDATE NO ACTION);

CREATE TABLE cook.produit
(nomProduit VARCHAR(20) NOT NULL,
catégorie ENUM('viande', 'poisson','oeuf', 'laitage', 'fruit', 'légume', 'condiment','féculent','huile','alcool', 'autre'),
unité ENUM('g','cueillere_a_soupe','unite','cL') NULL,
stockActuel FLOAT NULL,
stockMin FLOAT NULL,
stockMax FLOAT NULL,
PRIMARY KEY(nomProduit));

CREATE TABLE cook.compositionCommande
(numCommande VARCHAR(6) NOT NULL,
nomRecette VARCHAR(20) NOT NULL,
occurrencesRecette INT NULL,
PRIMARY KEY(nomRecette, numCommande),
CONSTRAINT nomRecette
 FOREIGN KEY(nomRecette)
 REFERENCES cook.recette(nomRecette) 
ON DELETE CASCADE ON UPDATE NO ACTION,
CONSTRAINT numCommande
 FOREIGN KEY(numCommande)
 REFERENCES cook.commande(numCommande) 
ON DELETE CASCADE ON UPDATE NO ACTION);

CREATE TABLE cook.fournisseur
(codeF VARCHAR(6) NOT NULL,
mdpF VARCHAR(24),
nomF VARCHAR(20) NOT NULL,
numTelF VARCHAR(10) NOT NULL,
PRIMARY KEY(codeF));

CREATE TABLE cook.cuisine
(cookID VARCHAR(6) NOT NULL,
nomRec VARCHAR(20) NOT NULL,
 datePrepa DATETIME NOT NULL,
 PRIMARY KEY(cookID, nomRec,datePrepa),
CONSTRAINT nomRec
 FOREIGN KEY(nomRec)
 REFERENCES cook.recette(nomRecette) 
ON DELETE CASCADE ON UPDATE NO ACTION,
CONSTRAINT cookID
 FOREIGN KEY(cookID)
 REFERENCES cook.cooking(idCook) 
ON DELETE CASCADE ON UPDATE NO ACTION);

CREATE TABLE cook.fournit
(codeF VARCHAR(5) NOT NULL,
nomProduit VARCHAR(20) NOT NULL,
dateF DATETIME NOT NULL,
quantiteF FLOAT NOT NULL,
PRIMARY KEY(codeF, nomProduit),
CONSTRAINT codeF
 FOREIGN KEY(codeF)
 REFERENCES cook.fournisseur(codeF) 
ON DELETE CASCADE ON UPDATE NO ACTION,
CONSTRAINT nomProduit
 FOREIGN KEY(nomProduit)
 REFERENCES cook.produit(nomProduit) 
ON DELETE CASCADE ON UPDATE NO ACTION);

CREATE TABLE cook.contenanceRecette
(recetteNom VARCHAR(20) NOT NULL,
 nomProd VARCHAR (20) NOT NULL,
 quantiteProduit FLOAT NOT NULL,
PRIMARY KEY(recetteNom,nomProd),
CONSTRAINT recetteNom
 FOREIGN KEY(recetteNom)
 REFERENCES cook.recette (nomRecette) 
ON DELETE CASCADE ON UPDATE NO ACTION,
CONSTRAINT nomProd
 FOREIGN KEY(nomProd)
 REFERENCES cook.produit(nomProduit) 
ON DELETE CASCADE ON UPDATE NO ACTION);

CREATE TABLE cook.contactFournisseur
 ( identifiantCook VARCHAR(5) NOT NULL,
 codeFournisseur VARCHAR(6) NOT NULL,
 dateAppel DATETIME NOT NULL,
PRIMARY KEY(identifiantCook,codeFournisseur),
CONSTRAINT identifiantCook
 FOREIGN KEY(identifiantCook) 
REFERENCES cook.cooking(idCook)
 ON DELETE CASCADE ON UPDATE NO ACTION,
CONSTRAINT codeFournisseur
 FOREIGN KEY(codeFournisseur)
 REFERENCES cook.fournisseur(codeF) 
ON DELETE CASCADE ON UPDATE NO ACTION);



INSERT INTO cook.cooking(idCook,mdpCook,nomCook,prenomCook,numTelCook) VALUES ('124','ZYRTZDi','Jean','Michel','012345689');
INSERT INTO cook.cooking(idCook,mdpCook,nomCook,prenomCook,numTelCook) VALUES ('352','34l2i','George','René','0198765432');

INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('jambon','viande','g',3,5,75);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('lait','laitage','cL',10,5,75);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('oeuf','oeuf','unite',20,6,25);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('lardons','viande','g',20,0.4,30);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('sel','condiment','cueillere_a_soupe',10,4,40);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('poivre','condiment','cueillere_a_soupe',50,10,100);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('pate brisée','féculent','unite',10,2,15);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('crème fraiche','laitage','cL',300,100,1000);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('tomate','fruit','g',2000,1000,6000);
INSERT INTO cook.produit(nomProduit,catégorie,unité,stockActuel,stockMin,stockMax) VALUES ('pomme de terre','légume','g',2000,1000,6000);

INSERT INTO cook.fournisseur(codeF,mdpF,nomF,numTelF) VALUES ('812','mdp','Jean Michel','02030523');
INSERT INTO cook.fournisseur(codeF,mdpF,nomF,numTelF) VALUES ('809','bug','Jean Marc','02030232');
INSERT INTO cook.fournisseur(codeF,mdpF,nomF,numTelF) VALUES ('805','mot','Jean Pierre','02330405');
select * from cook.client;



