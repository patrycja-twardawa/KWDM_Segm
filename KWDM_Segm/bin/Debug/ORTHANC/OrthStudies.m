function [studies] = OrthStudies(patient)
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    
    stud_temp = OrthancGetStudy(URL, patient); %% pobierz badania wybranego pacjenta (zapisywani w struct)
    
    for i = 1 : numel(stud_temp)
        studies(i,:) = stud_temp{i}.ID;
    end
end