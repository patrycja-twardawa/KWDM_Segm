function [patients] = OrthSeries()
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    pat_temp = OrthancGetPatients(URL); %% pobierz pacjentów
    
    for i = 1 : numel(pat_temp)
        stud_temp(i) = OrthancGetStudy(URL, pat_temp{i}); %% pobierz badania wybranego pacjenta (zapisywani w struct)
    end
    
    for i = 1 : numel(stud_temp)
        studies(i,:) = stud_temp{i}.ID;
        patient_names(i,:) = strcat(stud_temp{i}.PatientMainDicomTags.PatientName, repmat('_', 1, ...
            50-length(stud_temp{i}.PatientMainDicomTags.PatientName)));
        
        for j = 1 : length(stud_temp{1}.Series)
            series(i,:) = stud_temp{i}.Series;
        end
    end
    
    patients = unique(patient_names, 'rows');
end