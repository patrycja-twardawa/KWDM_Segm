function[instances, patient_info] = OrthInstances(study, serie_id)
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    
    series_temp = OrthancGetSeries(URL, study); %% pobierz badania wybranego pacjenta (zapisywani w struct)
    
    try
        d = strcat({'Data badania: '}, {series_temp{serie_id}.LastUpdate(7:8)}, {'-'}, ...
        {series_temp{serie_id}.LastUpdate(5:6)}, {'-'}, ...
        {series_temp{serie_id}.LastUpdate(1:4)}, {'; godz. '}, {series_temp{serie_id}.LastUpdate(10:11)}, {':'}, ...
        {series_temp{serie_id}.LastUpdate(12:13)}, {':'}, {series_temp{serie_id}.LastUpdate(14:15)});
    catch
        d = 'Brak daty.';
    end
    try
        m = strcat({'Modalnoœæ: '}, {series_temp{serie_id}.MainDicomTags.Modality});
    catch
        m = 'Brak informacji o modalnoœci.';
    end
    try
        s = strcat({'Status badania: '}, {series_temp{serie_id}.Status});
    catch
        s = 'Brak informacji o statusie.';
    end
    try
        b = strcat({'Zakres badania: '}, {series_temp{serie_id}.MainDicomTags.BodyPartExamined});
    catch
        b = 'Brak informacji o zakresie badania.';
    end
    
    patient_info(1,:) = strcat(char(d), repmat('_', 1, 60-length(char(d))));
    patient_info(2,:) = strcat(char(m), repmat('_', 1, 60-length(char(m))));
    patient_info(3,:) = strcat(char(s), repmat('_', 1, 60-length(char(s))));
    patient_info(4,:) = strcat(char(b), repmat('_', 1, 60-length(char(b))));
    
    instanc = OrthancGetInstance(URL, serie_id);
    
    for i = 1 : numel(instanc)
        instances(i,:) = instanc{1,i}.ID;
    end
end