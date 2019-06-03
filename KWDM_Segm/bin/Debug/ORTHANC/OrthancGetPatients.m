function [patients] = OrthancGetPatients(URL)
    addpath('./jsonlab');
    patients = loadjson(urlread([ URL '/patients/' ]));
end

