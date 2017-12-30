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
    write_running_log("main", logger)
    write_log("The promotion_sync will be executed daily", logger, True, False, print_color="green")
    aws_access_key_id = settings.get("amazon", "aws_access_key_id")
    aws_secret_access_key = settings.get("amazon", "aws_secret_access_key")
    aws_associate_tag = settings.get("amazon", "aws_associate_tag")
    aws_region = settings.get("amazon", "aws_region")
    df_node_file = settings.get("amazon", "df_node_file")
    output_folder = settings.get("amazon", "output_folder")
    tmp_folder = settings.get("amazon", "tmp_folder")
    aws_qps = settings.getfloat("amazon", "aws_qps")

    if args.force == "True":
        amazon = Amazon(logger, aws_access_key_id, aws_secret_access_key, aws_associate_tag, aws_region, aws_qps,
                        df_node_file, output_folder, tmp_folder)
        amazon.get_daily_promotions()
    while True:
        if pycron.is_now('0 10 * * *'):
            amazon = Amazon(logger, aws_access_key_id, aws_secret_access_key, aws_associate_tag, aws_region, aws_qps,
                            df_node_file, output_folder, tmp_folder)
            amazon.get_daily_promotions()
            time.sleep(60)
        else:
            time.sleep(60)


if __name__ == '__main__':
    usage = """
        python sync_deals.py
        The following parameters are mandatory:
            -c --config     The path to the configuration file for running the experiments
            -f --force The path to the configuration file for running the experiments
    """
    parser = argparse.ArgumentParser(description="Run the experiments.")
    parser.add_argument('-c', '--config', type=str, required=True)
    parser.add_argument('-f', '--force', type=str, default=False)
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
        write_log('Ready to start the sync', logger, True, False)

        main(logger, args, settings)

        write_running_log("main", logger)
