# Author: Stew Towle
# Date: November 2021
# Description: This script is for educational use only - NOT FOR COMMERCIAL USE
#               Can query definitions/thesaurus entries for up to 1000 words per day.
#               See README.md for official description of how to use.
#               request module must be installed on your machine to run this script
#               Run "pip install requests" to do so.

# Use: To use this script you must first create a text file called request.txt in the local directory
#      that this script is being called from (though for some forms of subprocess it needs to be where the script is saved)
#       The text file must contain only words you want definitions for, one word per line.
#       Running this script's main function will create json formatted text files, one per word requested
#       each with the name word.json where word is the requested word.
#       The result files will appear in the directory from which the subprocess call is made
#       For most ways of calling the script.

import requests

api_key = "2bfdeab3-0ae3-4e13-840e-a1e013d2b527"
mw_get_url = "https://dictionaryapi.com/api/v3/references/collegiate/json/"

def extract_definition(api_return_obj: list) -> list:
    """
    Given a json object that is the result of request to m-w api, returns a new list that is just
    a list of definition
    :param api_return_obj:
    :return:
    """
    result = list()
    #The following line parses out all of the short definition lists available for the given word
    list_defs = [api_return_obj[i]["shortdef"] for i in range(len(api_return_obj)) if "shortdef" in api_return_obj[i]]

    #Short definitions formatted as list of strings even when only one string in list
    # So we double loop withn a loop to access each string individually and append it to our definition list
    for def_str_list in list_defs:
        for a_def in def_str_list:
            result.append(a_def)

    return result



def make_request(url_str: str, key_dict: dict) -> list:
    """
    This function is for making the actual request to the api.
    :param url_str: url of api to request, including word to lookup
    :param key_dict: A dictionary with one entry, key is "key" and value is the api key
    :return: Returns a json formatted list object with dictionary data (provided the url given is actually for m-w api)
    """
    req_results = requests.get(url_str, key_dict)
    if req_results:
        #This uses requests special functionality of evaluating to true if status code is between 200 and 400
        return req_results.json()
    else:
        return "failed request"


def get_word_list():
    """
    Will always look in the local request.txt file to find words to look up
    :return:
    """
    list_words = list()
    try:
        with open("request.txt", "r") as file_reader:
            for line in file_reader.readlines():
                list_words.append(line.strip())
        return list_words
    except Exception:
        return "Definition unavailable. No 'request.txt' file found."
    return "Something went terribly wrong.  Definition unavailable."

def lookup_word(word: str) -> list:
    """
    Given a word to lookup will return the list of short definitions from m-w
    :param word:
    :return:
    """
    try:
        api_request = make_request(mw_get_url + word + "?", {"key": api_key})
    except Exception:
        api_request = "failed request"
    if api_request == "failed request":
        return ["Failed request to Merriam-Webster API: Definitions unavailable"]
    definitions = extract_definition(api_request)
    return definitions

def make_word_file(word: str, definitions: list) -> None:
    """Given a word and a list of definition for it
    will create a file named word.json (where word is replaced with the word)
    containing an array of strings in text format that are the definitions"""
    reformatted_defs = ["\"" + d + "\",\n" for d in definitions]
    reformatted_defs[0] = "[" + reformatted_defs[0]
    reformatted_defs[-1] = reformatted_defs[-1][:-3] + "\"]"
    with open(f"{word}.json", "w") as file_ref: #creates if doesnt exist, truncates if does
        for definition in reformatted_defs:
            file_ref.write(definition)

if __name__ == "__main__":
    words_to_lookup = get_word_list()
    if type(words_to_lookup) == str:
        make_word_file("microserviceError", [words_to_lookup])
    else:
        for word in words_to_lookup:
            the_defs = lookup_word(word)
            if the_defs:
                make_word_file(word, lookup_word(word))
            else:
                make_word_file(word, ["Word not found in the dictionary"])
