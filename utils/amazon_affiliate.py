# coding=utf-8

import bottlenose
import lxml
from bs4 import BeautifulSoup
import pandas as pd
import time
import numpy as np
from collections import OrderedDict, deque
import itertools
from tqdm import tnrange, tqdm_notebook, tqdm
import feedparser
import xmltodict
from flatten_dict import flatten
from datetime import datetime
import os
from utils.general_utils import *
import random
import time
from urllib.error import HTTPError
import shelve
tqdm.pandas(desc="my bar!")


def get_promotions(response, node_id):
    items = response.find("ItemSearchResponse").find("Items").findAll("Item")
    promos = []
    for item in items:
        res = OrderedDict({})
        res["node_id"] = node_id
        res["asin"] = item.find("ASIN").text
        res["title"] = find_prop(item, "Title")
        res["sales_rank"] = find_prop(item, "SalesRank")
        res["percentage_saved"] = find_prop(item, "PercentageSaved")
        res["amount_saved"] = find_prop(item, "AmountSaved")
        res["list_price"] = find_prop(item, "ListPrice")
        res["price"] = find_prop(item, "Price")
        res["availability"] = find_prop(item, "Availability")
        res["availability_type"] = find_prop(item, "AvailabilityType")
        res["is_eligible_for_prime"] = find_prop(item, "IsEligibleForPrime")
        res["is_eligible_for_super_saver_shipping"] = find_prop(item, "IsEligibleForSuperSaverShipping")
        res["eans"] = ",".join([f.get_text() for f in item.find_all("EANListElement")])
        res["features"] = "\n".join([f.get_text() for f in item.find_all("Feature")])
        res["features_brand"] = find_prop(item, "Brand")
        res["features_binding"] = find_prop(item, "Binding")
        res["features_department"] = find_prop(item, "Department")
        res["features_product_group"] = find_prop(item, "ProductGroup")
        res["features_release_date"] = find_prop(item, "ReleaseDate")
        promos.append(res)
    return promos


def find_children(root_id, request, nodes_name):
    nodes = []
    try:
        for node in request.find("Children").find_all("BrowseNode"):
            node_id = node.find("BrowseNodeId").text
            node_name = node.find("Name").text
            nodes_name[node_id] = node_name
            node = {"parent": root_id, "node_id": node_id}
            nodes.append(node)
    except Exception as ex:
        print(ex)
        pass
    return nodes


def error_handler(err):
    ex = err['exception']
    if isinstance(ex, HTTPError) and ex.code == 503:
        time.sleep(5 + np.random.randint(5))
        return True

class Amazon:
    def __init__(self, logger, aws_access_key_id, aws_secret_access_key, aws_associate_tag, aws_region, aws_qps,
                 df_node_file, output_folder, tmp_folder):

        self.logger = logger
        self.today = datetime.now().strftime("%Y_%m_%d")
        self.df_nodes = None
        self.df_products_clean = None
        self.df_rss = None
        self.df_node_file = df_node_file
        self.output_folder = output_folder
        self.shelve_file = os.path.join(tmp_folder, self.today)
        self.aws_access_key_id = aws_access_key_id
        self.aws_secret_access_key = aws_secret_access_key
        self.aws_associate_tag = aws_associate_tag
        self.aws_region = aws_region

        self.amazon = bottlenose.Amazon(self.aws_access_key_id,
                                        self.aws_secret_access_key,
                                        self.aws_associate_tag,
                                        Region=self.aws_region,
                                        Parser=lambda text: BeautifulSoup(text, features="xml"),
                                        ErrorHandler=error_handler, MaxQPS=aws_qps,
                                        CacheWriter=self.write_query_to_db,
                                        CacheReader=self.read_query_from_db)

        self.root_nodes = {"Bebé": 9482651011,
                           "Deportes y  Aire Libre": 9482661011,
                           "Electrónicos": 9482559011,
                           "Herramientas y  Mejoras del Hogar": 9482671011,
                           "Hogar y  Cocina": 9482594011,
                           "Libros": 9298577011,
                           "Música": 9482621011,
                           "Oficina y  Papelería": 9673845011,
                           "Películas y  Series de TV": 9482631011,
                           "Relojes": 9482681011,
                           "Salud, Belleza y Cuidado Personal": 9482611011,
                           "Software": 9482691011,
                           "Tienda Kindle": 6446440011,
                           "Videojuegos": 9482641011,
                           "Ropa": 14093027011}

        self.id_name = dict([(c[1], c[0]) for c in self.root_nodes.items()])

        self.id_eng = {9482651011: "Baby",
                       9298577011: "Books",
                       9482631011: "DVD",
                       9482559011: "Electronics",
                       9482611011: "HealthPersonalCare",
                       9482671011: "HomeImprovement",
                       6446440011: "KindleStore",
                       9482594011: "Kitchen",
                       9482621011: "Music",
                       9673845011: "OfficeProducts",
                       9482691011: "Software",
                       9482661011: "SportingGoods",
                       9482641011: "VideoGames",
                       9482681011: "Watches",
                       14093027011: "Fashion"}

        self.root_nodes = {"Bebé": 9482651011,
                           "Deportes y  Aire Libre": 9482661011,
                           "Electrónicos": 9482559011,
                           "Herramientas y  Mejoras del Hogar": 9482671011,
                           "Hogar y  Cocina": 9482594011,
                           "Libros": 9298577011,
                           "Música": 9482621011,
                           "Oficina y  Papelería": 9673845011,
                           "Películas y  Series de TV": 9482631011,
                           "Relojes": 9482681011,
                           "Salud, Belleza y Cuidado Personal": 9482611011,
                           "Software": 9482691011,
                           "Tienda Kindle": 6446440011,
                           "Videojuegos": 9482641011,
                           "Ropa": 14093027011}

        self.id_name = dict([(c[1], c[0]) for c in self.root_nodes.items()])

    def write_query_to_db(self, cache_url, data):
        with shelve.open(self.shelve_file) as db:
            db[cache_url] = data

    def read_query_from_db(self, cache_url):
        with shelve.open(self.shelve_file) as db:
            return db[cache_url] if cache_url in db.keys() else None

    def get_daily_promotions(self):
        write_running_log("get_daily_promotions", logger=self.logger)
        self.get_nodes()
        self.get_all_promotions()
        self.get_rss()

    def get_nodes(self):
        write_running_log("get_nodes", logger=self.logger)
        if os.path.exists(self.df_node_file):
            self.df_nodes = pd.read_csv(self.df_node_file)
        else:
            nodes = []
            nodes_name = {}
            all_nodes = []

            for root_name, root_id in self.root_nodes.items():
                nodes_name[root_id] = root_name
                print("Processing %s" % (root_name))
                request = self.amazon.BrowseNodeLookup(BrowseNodeId=root_id)
                try:
                    first_nodes = find_children(root_id, request, nodes_name)
                    second_nodes = [
                        find_children(n["node_id"], self.amazon.BrowseNodeLookup(BrowseNodeId=n["node_id"]), nodes_name) for n
                        in
                        first_nodes]
                    second_nodes = list(itertools.chain.from_iterable(second_nodes))
                    all_nodes += first_nodes + second_nodes
                except Exception as ex:
                    print(str(ex))
                    pass
            df_nodes = pd.DataFrame(all_nodes)
            df_nodes["node_name"] = df_nodes["node_id"].apply(lambda x: nodes_name[x])
            df_nodes["parent_name"] = df_nodes["parent"].apply(lambda x: nodes_name[x])
            dict_nodes = df_nodes[["node_id", "parent"]].set_index("node_id")["parent"].to_dict()
            parent_names = df_nodes[["parent", "parent_name"]].set_index("parent")["parent_name"].to_dict()
            df_nodes["root_node"] = df_nodes["parent"].apply(lambda x: parent_names.get(dict_nodes.get(x, ""), ""))
            df_nodes.to_csv(self.df_node_file, index=False)
            self.df_nodes = df_nodes

    def get_all_promotions(self):
        write_running_log("get_all_promotions", logger=self.logger)
        assert self.df_nodes is not None, "df_nodes should be loaded first"

        df_products_clean_file = os.path.join(self.output_folder, 'amazon_promotions_' + self.today + '.xlsx')

        if os.path.exists(df_products_clean_file):
            self.df_products_clean = pd.read_excel(df_products_clean_file, sheetname="promotions")
        else:
            d_items = deque()
            processed = set()

            for i, v in tqdm(self.df_nodes[~self.df_nodes["root_node"].isnull()].iterrows()):
                search_index = v["parent"]
                if search_index not in self.id_eng.keys():
                    search_index = self.df_nodes.query("node_id == @search_index").iloc[0]["parent"]
                    if search_index not in self.id_eng.keys():
                        search_index = self.df_nodes.query("node_id == @search_index").iloc[0]["parent"]
                use_id = v["node_id"]
                if use_id not in processed:
                    i = 1
                    response = self.amazon.ItemSearch(SearchIndex=self.id_eng[search_index],
                                                      BrowseNode=use_id,
                                                      ResponseGroup="SalesRank,Images,ItemAttributes,Offers,PromotionSummary",
                                                      Sort="relevancerank",
                                                      MinimumPrice="600",
                                                      MinPercentageOff="10",
                                                      Condition="New",
                                                      Availability="Available",
                                                      MerchantId="Amazon")
                    d_items.append(get_promotions(response, use_id))
                    pages = list(range(2, min(5, int(response.find("TotalPages").text)) + 1, 1))
                    for page in pages:
                        response = self.amazon.ItemSearch(SearchIndex=self.id_eng[search_index],
                                                          BrowseNode=use_id,
                                                          ResponseGroup="SalesRank,Images,ItemAttributes,Offers,PromotionSummary",
                                                          Sort="relevancerank",
                                                          MinimumPrice="600",
                                                          MinPercentageOff="10",
                                                          ItemPage=page)
                        d_items.append(get_promotions(response, use_id))
                    processed.add(use_id)

            df_products = pd.DataFrame(list(itertools.chain.from_iterable(d_items)))

            df_products_clean = df_products.query("percentage_saved != '' and amount_saved != '' and list_price != '' ")

            df_products_clean["percentage_saved"] = df_products_clean["percentage_saved"].apply(
                lambda x: x.split("MXNMXN")[0])
            df_products_clean["amount_saved"] = df_products_clean["amount_saved"].apply(lambda x: x.split("MXNMXN")[0])
            df_products_clean["list_price"] = df_products_clean["list_price"].apply(lambda x: x.split("MXNMXN")[0])
            df_products_clean["price"] = df_products_clean["price"].apply(lambda x: x.split("MXNMXN")[0])

            df_products_clean["percentage_saved"] = df_products_clean["percentage_saved"].astype(int)
            df_products_clean["amount_saved"] = df_products_clean["amount_saved"].astype(int)
            df_products_clean["list_price"] = df_products_clean["list_price"].astype(int)
            df_products_clean["price"] = df_products_clean["price"].astype(int)

            df_products_clean["percentage_saved"] = df_products_clean["percentage_saved"] / 100
            df_products_clean["amount_saved"] = df_products_clean["amount_saved"] / 100
            df_products_clean["list_price"] = df_products_clean["list_price"] / 100
            df_products_clean["price"] = df_products_clean["price"] / 100

            df_products_clean["sales_rank"] = df_products_clean["sales_rank"].fillna("99999999")
            df_products_clean["sales_rank"] = df_products_clean["sales_rank"].apply(
                lambda x: "99999999" if x == "" else x)
            df_products_clean["sales_rank"] = df_products_clean["sales_rank"].astype(int)

            df_products_clean = df_products_clean.drop_duplicates()

            df_products_clean = df_products_clean.sort_values("amount_saved", ascending=False)

            df_products_clean = df_products_clean.merge(self.df_nodes, on="node_id")

            writer = pd.ExcelWriter(df_products_clean_file)
            df_products_clean.to_excel(writer, 'promotions')
            self.df_products_clean = df_products_clean
            self.df_nodes.to_excel(writer, 'categories')
            writer.save()

    def get_rss(self):
        write_running_log("get_rss", logger=self.logger)
        assert self.df_nodes is not None and self.df_products_clean is not None, "df_nodes and df_products_clean " \
                                                                                 "should be loaded first"

        df_rss_file = os.path.join(self.output_folder, 'amazon_promotions_hot_' + self.today + '.xlsx')

        if os.path.exists(df_rss_file):
            self.df_rss = pd.read_excel(df_rss_file, sheetname="hot_promotions")
        else:
            all_rss = {}
            for i, v in tqdm(self.df_nodes[~self.df_nodes["root_node"].isnull()].iterrows()):
                node_rss = {}
                if v["root_node"] in self.root_nodes.keys():
                    cat = self.root_nodes[v["root_node"]]
                else:
                    root_node = self.df_nodes[self.df_nodes["parent_name"] == v["root_node"]].iloc[0]["root_node"]
                    cat = self.root_nodes[root_node]
                eng = self.id_eng[cat]
                node_id = v["node_id"]
                for rss_type in ['bestsellers', 'new-releases', 'movers-and-shakers']:
                    d = feedparser.parse(
                        "https://www.amazon.com.mx/rss/%s/%s/%s?tag=%s" % (
                            rss_type, eng.lower(), node_id, self.aws_associate_tag))
                    if len(d["entries"]) > 1:
                        node_rss[rss_type] = [e.link.split("/dp/")[1].split("/")[0] for e in d["entries"]]
                    all_rss[node_id] = node_rss

            df_rss = []
            for key, value in all_rss.items():
                for key_2, value_2 in value.items():
                    for item in value_2:
                        res = OrderedDict({})
                        res["node_id"] = key
                        res["rss_type"] = key_2
                        res["asin"] = item
                        df_rss.append(res)
            df_rss = pd.DataFrame(df_rss)
            df_rss = df_rss.merge(self.df_products_clean, on="asin")

            df_rss["sales_rank"] = df_rss["sales_rank"].fillna("99999999")
            df_rss["sales_rank"] = df_rss["sales_rank"].apply(lambda x: "99999999" if x == "" else x)
            df_rss["sales_rank"] = df_rss["sales_rank"].astype(int)

            writer = pd.ExcelWriter(df_rss_file)
            df_rss.sort_values(["root_node", "parent_name", "node_name", "features_product_group"]).to_excel(writer,
                                                                                                             'hot_promotions')

            writer.save()
            self.df_rss = df_rss
