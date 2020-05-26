# Blazorme.Diff

Diff component library for Blazor apps. 

## Introduction
The idea is to render diff of the two input strings in different output display formats. Output is always in HTML and currently 3 output display formats are provided:
* Inline: Inlined format. Intented for HTML inputs although plain text can also be used.
* Row: Line by line format for text inputs.
* Column: Side by side format for text inputs.


![alt text](https://github.com/melihercan/gifs/blob/master/Diff.gif)


## Installation

## Component Usage
In your Blazor page add:
### For Inline format
No need to specify titles for inline. They are not used.
Also OutputFormat by default is Inline, hence no need to specify the OutputFormat.
```html
  <Diff FirstInput="first input text"
        SecondInput="second input text"/>
```
### For Row format
```html
  <Diff FirstInput="first input text"
        SecondInput="second input text"
        FirstTitle="first title text"
        SecondTitle="second title text"
        OutputFormat=@DiffOutputFormat.Row />
```
### For Column format
```html
  <Diff FirstInput="first input text"
        SecondInput="second input text"
        FirstTitle="first title text"
        SecondTitle="second title text"
        OutputFormat=@DiffOutputFormat.Column />
```



## API Usage


