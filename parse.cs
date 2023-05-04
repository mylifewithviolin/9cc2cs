using System; 
using System.Text.RegularExpressions;



namespace CC9
{
    public partial class Program
    {

        const string TOKEN_SYNBL_PLUS ="+";
        const string TOKEN_SYNBL_MINUS ="-";
        // トークンの種類
        enum TokenKind{
            TK_RESERVED, // 記号
            TK_NUM,      // 整数トークン
            TK_EOF,      // 入力の終わりを表すトークン
        };

        // 抽象構文木のノードの種類
        enum NodeKind{
            ND_ADD, // +
            ND_SUB, // -
            ND_MUL, // *
            ND_DIV, // /
            ND_EQ,  // ==
            ND_NE,  // !=
            ND_LT,  // <
            ND_LE,  // <=
            ND_NUM, // 整数
        };

        class Token {
            public TokenKind kind; // トークンの型
            public  int next;    // 次の入力トークン
            public int val;        // kindがTK_NUMの場合、その数値
            public string? str;      // トークン文字列
            public int len;        // トークンの長さ
            
        };
    
        // 抽象構文木のノードの型
        class  Node {
            public NodeKind kind; // ノードの型
            public Node? lhs;     // 左辺
            public Node? rhs;     // 右辺
            public int val;       // kindがND_NUMの場合のみ使う
        };

        // 入力文字列pをトークナイズしてそれを返す
        // 入力文字列をトークナイズしてトークンリストを返す
        /// <summary>トークナイズする</summary>
        /// <param name="p">入力文字列</param>
        /// <returns>トークンリスト</returns>
        static List<Token> tokenize(string p) {
            int next = 1;
            Token cur = new();
            List<Token> tokenList =new List<Token>();

            var cs = p.ToCharArray();
            int i= 0;
            while(i<cs.Length) {
                // 空白文字をスキップ
                if (isspace(cs[i].ToString())) {
                    i++;
                    continue;
                }
                // Multi-letter punctuator
                if (iscomparisonhead(cs[i].ToString())) {
                    if(i+1<cs.Length){
                        var comp =cs[i].ToString()+cs[i+1].ToString();
                        if (iscomparison(comp)) {
                            cur = new_token(TokenKind.TK_RESERVED, next, comp,2);
                            tokenList.Add(cur);
                            next++;
                            i+=2;
                            continue;
                        }
                    }
                }

                // Single-letter punctuator
                if (isarithmetic(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_RESERVED, next, cs[i].ToString(),1);
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                if (isdigit(cs[i].ToString())) {
        
                    string numberStr = cs[i].ToString();
                    i++;
                    if(i<cs.Length){
                        while(isdigit(cs[i].ToString())) {
                            numberStr+= cs[i].ToString();
                            i++;
                            if(i>=cs.Length)break;
                        }
                    }
                    cur = new_token(TokenKind.TK_NUM,next, numberStr,i);
                    cur.val = strtol(numberStr);
                    tokenList.Add(cur);
                    next++;
                    continue;
                }

                //ここに進んだら例外
                //Console.Write($"トークナイズできません");
                i++;
            }

            cur = new_token(TokenKind.TK_EOF, next, "",0);
            tokenList.Add(cur);
            return tokenList;
        }


        /// <summary>式</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>等式</returns>
        static Node expr(List<Token> tokenList,ref int curIndex) {
            return equality(tokenList,ref curIndex);
        }
        /// <summary>等式</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>ノード</returns>
        static Node equality(List<Token> tokenList,ref int curIndex) {
            Node node = relational(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//次のトークン
            for (;;) {
                if (consume(token,"-=",ref curIndex)){
                    node = new_binary(NodeKind.ND_EQ, node, relational(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                }
                else if (consume(token,"!=",ref curIndex)){
                    node = new_binary(NodeKind.ND_NE, node, relational(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                }
                else
                return node;
            }
        }

        /// <summary>関係式</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>ノード</returns>
        static Node relational(List<Token> tokenList,ref int curIndex) {
            Node node = add(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//次のトークン
            for (;;) {
                if (consume(token,"<",ref curIndex)){
                                    node = new_binary(NodeKind.ND_LT, node, primary(tokenList,ref curIndex));
                                token = getToken(tokenList,curIndex);//次のトークン
                }
                else if (consume(token,"<=",ref curIndex)){
                    node = new_binary(NodeKind.ND_LE, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else if (consume(token,">",ref curIndex)){
                    node = new_binary(NodeKind.ND_LT, add(tokenList,ref curIndex), node);
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else if (consume(token,">=",ref curIndex)){
                    node = new_binary(NodeKind.ND_LE, add(tokenList,ref curIndex), node);
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else
                return node;
            }
        }
        /// <summary>加減式</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>ノード</returns>
        static Node add(List<Token> tokenList,ref int curIndex) {
            Node node = mul(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//次のトークン
            for (;;) {
                if (consume(token,"+",ref curIndex)){
                    //node = new_node(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else if (consume(token,"-",ref curIndex)){
                    //node = new_node(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                    }
                else
                    return node;
            }
        }
        /// <summary>乗除式</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>ノード</returns>
        static Node mul(List<Token> tokenList,ref int curIndex) {
            //Node node = primary(tokenList,ref curIndex);
            Node node = unary(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//次のトークン
            for (;;) {
                if (consume(token,"*",ref curIndex)){
                    //node = new_node(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                }
                else if (consume(token,"/",ref curIndex)){
                    //node = new_node(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//次のトークン
                }
                else
                return node;
            }
        }
        /// <summary>単項式</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>ノード</returns>
        static Node unary(List<Token> tokenList,ref int curIndex) { 
                Token token = getToken(tokenList,curIndex);//次のトークン
                if (consume(token,"+",ref curIndex)){
                    return unary(tokenList,ref curIndex);
                }
                else if (consume(token,"-",ref curIndex)){
                    return new_binary(NodeKind.ND_SUB,new_num(0), unary(tokenList,ref curIndex));
                }
                else
                return primary(tokenList,ref curIndex);
        }
        /// <summary>優先順位</summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="curIndex">現索引</param>
        /// <returns>ノード</returns>
        static Node primary(List<Token> tokenList,ref int curIndex) {
            Token token = getToken(tokenList,curIndex);//次のトークン
            // 次のトークンが"("なら、"(" expr ")"のはず
            if (consume(token,"(",ref curIndex)) {
                Node node = expr(tokenList,ref curIndex);
                token = getToken(tokenList,curIndex);//次のトークン
                expect(token,")",ref curIndex);
                token = getToken(tokenList,curIndex);//次のトークン
                return node;
            }

            // そうでなければ数値のはず
            //return new_node_num(expect_number(token,ref curIndex));
            return new_num(expect_number(token,ref curIndex));
        }

            static Token getToken(List<Token> tokenList,int curIndex) {
                if(curIndex>=tokenList.Count) return new Token();
                Token token = tokenList.ElementAt(curIndex);//次のトークン
                return token;
            }
            /// <summary>新しいノード</summary>
            /// <param name="kind">種類</param>
            /// <returns>ノード</returns>
            static Node new_node(NodeKind kind) {
                Node node = new();
                node.kind = kind;
                // node.lhs = lhs;
                // node.rhs = rhs;
                return node;
            }
            /// <summary>新しい二分木</summary>
            /// <param name="kind">種類</param>
            /// <param name="lhs">左辺</param>
            /// <param name="rhs">右辺</param>
            /// <returns>node</returns>
            static Node new_binary(NodeKind kind, Node lhs, Node rhs) {
                Node node = new_node(kind);
                node.lhs = lhs;
                node.rhs = rhs;
                return node;
            }


            static Node new_num(int val) {
                Node node = new_node(NodeKind.ND_NUM);
                node.val = val;
                return node;
            }


            // 次のトークンが期待している記号のときには、トークンを1つ読み進めて
            // 真を返す。それ以外の場合には偽を返す。
            /// <summary>使用する</summary>
            /// <param name="token">トークン</param>
            /// <param name="op">記号文字</param>
            /// <param name="next">次索引</param>
            /// <returns>bool</returns>
            static bool consume(Token token,string op, ref int next) {
                //if (token.kind != TokenKind.TK_RESERVED || token.str != op)
                if (token.kind != TokenKind.TK_RESERVED || op.Length  != token.len || 
                        token.str != op)
                    return false;
                next = token.next;
                return true;
            }
            // 次のトークンが期待している記号のときには、トークンを1つ読み進める。
            // それ以外の場合にはエラーを報告する。
            /// <summary>予想する</summary>
            /// <param name="token">トークン</param>
            /// <param name="op">記号文字</param>
            /// <param name="next">次索引</param>
            /// <returns>bool</returns>
        static  void expect(Token token,string op, ref int next) {
            if (token.kind != TokenKind.TK_RESERVED || op.Length  != token.len ||  token.str != op){
                       // Console.WriteLine($"{op}ではありません");
                    }
            next = token.next;
        }

        // 次のトークンが数値の場合、トークンを1つ読み進めてその数値を返す。
        // それ以外の場合にはエラーを報告する。
        static int expect_number(Token token, ref int next) {
            if (token.kind != TokenKind.TK_NUM){
                //Console.WriteLine($"数ではありません");
            }
            int val = token.val;
            next = token.next;
            return val;
        }

        // エラーを報告するための関数
        static void error(string fmt) {
            Console.WriteLine(fmt);
        }
        static bool at_eof(Token token) {
            return token.kind == TokenKind.TK_EOF;
        }

        // 新しいトークンを作成してcurに繋げる
            static Token new_token(TokenKind kind, int next, string str, int len) {
            Token tok = new();
            tok.kind = kind;
            tok.str = str;
            tok.len = len;
            tok.next = next;
            return tok;
        }
        static bool isspace( string input )
        {
            return( Regex.IsMatch( input,"\\s" ) );
        }
        static bool isarithmetic( string input )
        {
            return( Regex.IsMatch( input,"[\\+\\-\\*\\/\\(\\)\\<\\>]" ) );
        }
        static bool iscomparisonhead ( string input )
        {
            if(input =="="||input =="!"||input =="<"||input ==">"){
                return true;
            }else{
                return false;
            }       
        }

        static bool iscomparison ( string input )
        {
            if(input =="=="||input =="!="||input =="<="||input ==">="){
                return true;
            }else{
                return false;
            }       
        }

        static bool isdigit( string input )
        {
            return Regex.IsMatch( input, "[0-9]" );
        }
        static int strtol(string str){
            return  int.Parse(str);
        }

            
    }
}