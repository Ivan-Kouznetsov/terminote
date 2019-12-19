#include <iostream>
#include <chrono>
#include <iomanip>
#include <cmath>
#include <fstream>
#include <sstream>
#include <stdexcept>
#include <vector>

#include <sys/ioctl.h> // for ioctl() and TIOCGWINSZ
#include <unistd.h> // for STDOUT_FILENO

#define UP_ESCAPE "\x1b[1A"
#define RETURN_TO_START_ESCAPE "\r"

using namespace std;

void tryPrintLines(string filename, unsigned long lineCount) {
	fstream read;
	read.open(filename, fstream::in);
	string currentLine;

	vector<string> lines;

	if (read) {
		try {
			while (getline(read, currentLine)) {
				lines.push_back(currentLine);
			}
		}
		catch (exception e) {
			cout << e.what();
			read.close();
		}

		lineCount = min(lineCount, lines.size());

		for (unsigned long i = lines.size() - lineCount; i < lines.size(); i++) {
			cout << lines.at(i) << endl;
		}
	}

	read.close();
}

int getLineOffset(string str) {
	struct winsize consoleSize;
	ioctl(STDOUT_FILENO, TIOCGWINSZ, &consoleSize);

	double consoleColumns = consoleSize.ws_col;
	double stringLength = str.size();

	return static_cast<int>(ceil(stringLength / consoleColumns));
}

int main(int argc, char* argv[])
{
	const string prompt = "terminote> ";
	const string defaultFilename = "terminote.log";
	const unsigned long linesToShow = 10;

	string filename = defaultFilename;
	string userInput;

	if (argc == 1)
	{
		cout << defaultFilename << endl;
	}
	else if (argc == 2) {
		filename = argv[1];
	}
	else {
		cout << "Usage: terminote [filename]";
	}

	tryPrintLines(filename, linesToShow);

	fstream notesFile;
	notesFile.open(filename, fstream::out | fstream::app);

	while (1)
	{
		cout << prompt;
		getline(cin, userInput);

		if (userInput == ":q")
		{
			notesFile.close();
			return EXIT_SUCCESS;
		}

		time_t now = chrono::system_clock::to_time_t(chrono::system_clock::now());
		stringstream timestamp;
		timestamp << put_time(localtime(&now), "%F %T");

		string currentEntry = timestamp.str() + "> " + userInput;

		for (int i = 0; i < getLineOffset(prompt + userInput); i++) {
			cout << UP_ESCAPE;
		}

		cout << RETURN_TO_START_ESCAPE;

		//write to file
		try {
			notesFile << currentEntry + '\n';
		}
		catch (exception const& e) {
			cout << e.what();
			return EXIT_FAILURE;
		}

		cout << currentEntry << endl;
	}
}
