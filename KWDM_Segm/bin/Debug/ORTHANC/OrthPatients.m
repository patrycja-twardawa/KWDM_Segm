function [patients, patients_id] = OrthPatients()
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    pat_temp = OrthancGetPatients(URL); %% pobierz pacjentów
    
    for i = 1 : numel(pat_temp)
        stud_temp{i} = OrthancGetStudy(URL, pat_temp{i}); %% pobierz badania wybranego pacjenta (struct)
    end
    
    for i = 1 : numel(stud_temp)
        studies{i} = stud_temp{1,i}{1,1}.ID;
        patient_names(i,:) = strcat(stud_temp{1,i}{1,1}.PatientMainDicomTags.PatientName, repmat('_', 1, ...
            50-length(stud_temp{1,i}{1,1}.PatientMainDicomTags.PatientName)));
        patients_id(i,:) = stud_temp{1,i}{1,1}.ParentPatient;
    end
    
    [patients, ia, ic] = unique(patient_names, 'rows');
    %patients_id = unique(patients_id, 'rows');
    patients_id = patients_id(ia,:);
end