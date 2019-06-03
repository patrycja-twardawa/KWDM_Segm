function [studies] = OrthancGetStudy(URL, patient)
    addpath('./jsonlab');
    studies = loadjson(urlread([ URL '/patients/' patient '/studies/' ]));
end
