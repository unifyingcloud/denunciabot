"""
This module takes the raw data, analyzed data and generates a set of posts containing the promotions.
"""

import os
import json
import pandas as pd
import argparse
import configparser
import logging
from utils.general_utils import *
import pycron
from utils.amazon_affiliate import Amazon


def main(logger, args, settings):
    return None


if __name__ == '__main__':
    usage = """
        python post_recommendations.py
        The following parameters are mandatory:
            -c --config     The path to the configuration file for running the experiments
    """
    parser = argparse.ArgumentParser(description="Run the experiments.")
    parser.add_argument('-c', '--config', type=str, required=True)
    args = parser.parse_args()
    if not os.path.isfile(args.config):
        print(usage)
        exit("The configuration file does not exists")
    else:
        # Load the settings
        settings = configparser.ConfigParser()
        settings._interpolation = configparser.ExtendedInterpolation()
        settings.read(args.config)

        # Set-up the logging
        root = logging.getLogger()
        for handler in root.handlers[:]:
            root.removeHandler(handler)

        debug_level = settings.get("settings", "debug_level")
        if debug_level == "DEBUG":
            level = logging.DEBUG
            print("LOGGING AT DEBUG LEVEL")
        else:
            level = logging.INFO
            print("LOGGING AT INFO LEVEL")

        logging.basicConfig(level=level, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
                            filename=args.config.split("/")[-1].replace(".config", ".log"))
        logger = logging.getLogger(__name__)
        write_log('Ready to start the recommendations', logger, True, False)

        main(logger, args, settings)

        write_running_log("main", logger)
