addpath('.\jsonlab');
URL = 'http://localhost:8042'; %% adres serwera

patients = OrthancGetPatients(URL); %% pobierz pacjentów
studies = OrthancGetStudy(URL, patients{1}); %% pobierz badania wybranego pacjenta (zapisywani w struct)
%series = OrthancGetSeries(URL, study{1}.ID); %% pobierz serie wybranego badania 
%instances = OrthancGetInstance(URL, series{1}.ID); %% pobierz instancje serii

%rep = OrthancDownloadInstance(URL,instances{1}.ID) %% download (zapisuje w aktualnym folderze)
