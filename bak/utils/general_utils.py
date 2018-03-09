# coding=utf-8

# import random
import pandas as pd
import io
import json
from dateutil import parser
from collections import OrderedDict
import time
import csv
import difflib
import matplotlib
import numpy as np
import matplotlib.pyplot as plt
import os
import sys, traceback
from matplotlib.legend_handler import HandlerLine2D
import multiprocessing
import argparse
import logging
import configparser
import math
from termcolor import colored
import pickle
from collections import deque
from tqdm import tqdm
tqdm.pandas(desc="my bar!")


def find_prop(item, prop):
    find = item.find(prop)
    return find.text if find is not None else ""


def write_pickle(x, filename):
    with open(filename, 'wb') as f:
        pickle.dump(x, f)


def read_pickle(filename):
    with open(filename, 'rb') as f:
        x = pickle.load(f)
    return x


def write_log(message, logger, print_too=True, is_error=False, print_color="magenta"):
    logger.info(message) if not is_error else logger.error(message)
    if print_too:
        if not is_error:
            print(colored(message, print_color))
        else:
            print(colored(message, "red"))


def write_running_log(method, logger):
    write_log("********** Executing " + method, logger, True, False, print_color="blue")


def save2csv(df, filename):
    df = df.to_dataframe() if not isinstance(df, pd.DataFrame) else df
    df.to_csv(filename, index=False, encoding="utf8", quoting=csv.QUOTE_ALL, quotechar="\"")


def csv2pandas(filename):
    return pd.read_csv(filename, encoding="utf8", quoting=csv.QUOTE_ALL, quotechar="\"")


def save_plot(fig, save_to):
    if os.path.isfile(save_to):
        os.remove(save_to)
    fig.savefig(save_to + ".png", format="png", bbox_inches="tight", dpi=300)