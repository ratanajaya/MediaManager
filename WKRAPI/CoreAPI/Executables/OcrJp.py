import sys
from manga_ocr import MangaOcr

# Retrieve the image path from command-line arguments
image_path = sys.argv[1]  # sys.argv[0] is the script name, sys.argv[1] is the first argument

def get_text(image_path):
    mocr = MangaOcr()
    text = mocr(image_path)
    return text

# Call the function and print the result so C# can read it
if __name__ == "__main__":
    ocr_result = get_text(image_path)
    print(ocr_result)
